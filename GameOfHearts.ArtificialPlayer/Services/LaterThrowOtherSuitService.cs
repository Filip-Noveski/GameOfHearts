using GameOfHearts.ArtificialPlayer.Enums;
using GameOfHearts.ArtificialPlayer.Extensions;
using GameOfHearts.ArtificialPlayer.Models;
using GameOfHearts.ArtificialPlayer.Utilities;
using GameOfHearts.Game.Models;
using GameOfHearts.Game.Services;
using GameOfHearts.LoggingProvider.Extensions;
using GameOfHearts.LoggingProvider.Services;

namespace GameOfHearts.ArtificialPlayer.Services;

public sealed class LaterThrowOtherSuitService
{
    private static readonly int PercentageUtilityFactorEliminateSuit = 50;
    private static readonly string LoggerPrefix = "[later throw, other suit]";

    private readonly CardsUtilities _cardUtilities;
    private readonly Logger _logger;

    public LaterThrowOtherSuitService(Logger logger)
    {
        _logger = logger;
        _cardUtilities = new();
    }

    /// <summary>
    /// Calculates the utilities of the cards of the provided suit and returns a Decisions-Dictionary.
    /// </summary>
    /// <param name="cards">The cards to be calculated. All should be of the same suit.</param>
    /// <param name="representaionPercentage">The percentage of the suit in the whole pool of cards (all suits).</param>
    /// <param name="threshold">The percentage of cards below which elimination of the suit is desired.</param>
    /// <returns></returns>
    private Dictionary<Decision<LaterThrowOtherSuitStrategy>, int> CalculateUtilitiesEliminateSuit(
        ReadOnlySpan<Card> cards, float representaionPercentage, float threshold)
    {
        if (representaionPercentage > threshold || representaionPercentage <= 0)
        {
            // elimination not desired or no cards included, skip
            _logger.LogInformation(
                $"{LoggerPrefix} Represantion ({representaionPercentage}) is greater than threshold or zero, skipping...");
            return new();
        }

        // lower percentage should be eliminated first, hence threshold - percentage;
        // divide by threshold to get closer to 1.0
        int baseSuitUtility = PercentageUtilityFactorEliminateSuit 
            * (int)((threshold - representaionPercentage) / threshold);
        Dictionary<Decision<LaterThrowOtherSuitStrategy>, int> decisions = new();

        foreach (Card card in cards)
        {
            int cardUtility = baseSuitUtility + CardService.GetCardWeight(card);
            Decision<LaterThrowOtherSuitStrategy> decision = new(
                card, LaterThrowOtherSuitStrategy.EliminateSuitWithFewCards);

            decisions.Add(decision, cardUtility);
            _logger.LogInformation($@"
                {LoggerPrefix} Registered possible decision '{decision.Strategy}' on '{decision.Card}' with 
                utility {cardUtility}'
                ");
        }

        return decisions;
    }

    private Dictionary<Decision<LaterThrowOtherSuitStrategy>, int> GenerateEliminateSuitDecisions(
        ReadOnlySpan<Card> diamonds, 
        ReadOnlySpan<Card> spades, 
        ReadOnlySpan<Card> clover)
    {
        int total = diamonds.Length + spades.Length + clover.Length;
        float diamondsPercent = diamonds.Length / (float)total;
        float spadesPercent = spades.Length / (float)total;
        float cloverPercent = clover.Length / (float)total;

        if (diamondsPercent is 1f || spadesPercent is 1f || cloverPercent is 1f) 
        {
            // only one suit included, use other strategy
            return new();
        }

        // if one suit is already eliminated, try eliminate suit representing
        // less than 35% of cards; else suit representing less than 22% of cards
        float threshold = diamondsPercent is 0f || spadesPercent is 0f || cloverPercent is 0f
            ? 0.35f 
            : 0.22f;
        _logger.LogInformation($"{LoggerPrefix} Have multiple suits, threshold is: {threshold}");
        Dictionary<Decision<LaterThrowOtherSuitStrategy>, int> decisions = new();

        decisions.AddRange(CalculateUtilitiesEliminateSuit(diamonds, diamondsPercent, threshold));
        decisions.AddRange(CalculateUtilitiesEliminateSuit(spades, spadesPercent, threshold));
        decisions.AddRange(CalculateUtilitiesEliminateSuit(clover, cloverPercent, threshold));

        return decisions;
    }

    private Dictionary<Decision<LaterThrowOtherSuitStrategy>, int> GenerateEliminateHighCardDecisions(
        List<Card> hand)
    {
        Dictionary<Decision<LaterThrowOtherSuitStrategy>, int> decisions = new();
        foreach (Card card in hand)
        {
            Decision<LaterThrowOtherSuitStrategy> decision = new(card, LaterThrowOtherSuitStrategy.EliminateHighCards);
            int utility = CardService.GetCardWeight(card);
            decisions.Add(decision, utility);
            _logger.LogInformation($@"
                {LoggerPrefix} Registered possible decision '{decision.Strategy}' on '{decision.Card}' with 
                utility {utility}'
                ");
        }

        return decisions;
    }

    public Card MakeDecision(List<Card> hand)
    {
        IOrderedEnumerable<Card> orderedHand = hand.OrderBy(x => x.Suit)
            .ThenBy(x => x.Face);
        _logger.LogInformation($"{LoggerPrefix} Ordered hand: {orderedHand.Stringify()}");

        _cardUtilities.SegmentIntoSpans(
            orderedHand,
            out var hearts,
            out var diamonds,
            out var clover,
            out var spades);

        if (hearts.Length > 0)
        {
            Card card = hearts[^1]; // last, should be highest
            _logger.LogInformation($"{LoggerPrefix} Have hearts, throwing: {card}");
            return card;  
        }

        // prefer eliminating suits
        var decisions = GenerateEliminateSuitDecisions(diamonds, spades, clover);
        if (decisions.Count > 0)
        {
            _logger.LogInformation($"{LoggerPrefix} Can eliminate suit, filtering optimal and returning");
            // find optimal decisions and return first (if multiple match,
            // chosen should not matter)
            return DecisionFilterUtilities.FilterOptimal(decisions)
                .First()
                .Card;
        }

        decisions = GenerateEliminateHighCardDecisions(hand);
        _logger.LogInformation($"{LoggerPrefix} Returning eliminatee high card decision");
        return DecisionFilterUtilities.FilterOptimal(decisions)
            .First()
            .Card;
    }
}
