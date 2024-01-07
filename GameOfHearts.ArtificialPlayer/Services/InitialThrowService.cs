using GameOfHearts.ArtificialPlayer.Enums;
using GameOfHearts.ArtificialPlayer.Models;
using GameOfHearts.ArtificialPlayer.Utilities;
using GameOfHearts.Game.Enums;
using GameOfHearts.Game.Models;
using GameOfHearts.Game.Services;
using GameOfHearts.LoggingProvider.Extensions;
using GameOfHearts.LoggingProvider.Services;
using System.Diagnostics;

namespace GameOfHearts.ArtificialPlayer.Services;

public sealed class InitialThrowService
{
    private static readonly int MaxCardWeight = 15;
    private static readonly int MaxUtility = 14;
    private static readonly string LoggerPrefix = "[initial throw]";

    private readonly CardsUtilities _cardUtilities;
    private readonly Logger _logger;

    public InitialThrowService(Logger logger)
    {
        _logger = logger;
        _cardUtilities = new();
    }

    private void AddUtilitiesOverCard(
        Dictionary<Decision<InitialThrowStrategy>, int> utilities, 
        Card card, 
        List<Card> memory)
    {
        if (card.Suit is CardSuit.Heart)
        {
            InitialThrowStrategy strategy = InitialThrowStrategy.LowHeart;
            Decision<InitialThrowStrategy> decision = new(card, strategy);
            int individualCardWeight = MaxCardWeight - CardService.GetCardWeight(card);
            Card? optimal = _cardUtilities.GetHighestCardNonBreakingFrom2(
                memory.FindAll(x => x.Suit == CardSuit.Heart));
            // optimal utility should be 14, no cards remembered should add 1 as weight(2) = 2
            int memoryBenefit = optimal is null ? 1 : CardService.GetCardWeight(optimal);
            
            utilities.Add(decision, individualCardWeight + memoryBenefit);
            _logger.LogInformation($@"
                {LoggerPrefix} Registered possible decision '{decision.Strategy}' on '{decision.Card}' with 
                individual card weight '{individualCardWeight}' and memory benefit '{memoryBenefit}'
                ");
        }

        if (card.Suit is not CardSuit.Heart)
        {
            InitialThrowStrategy strategy = InitialThrowStrategy.TryTake;
            Decision<InitialThrowStrategy> decision = new(card, strategy);
            int individualCardWeight = CardService.GetCardWeight(card);
            Card? optimal = _cardUtilities.GetLowertCardNonBreakingFromAce(
                memory.FindAll(x => x.Suit == card.Suit));
            // optimal utility should be 14, no cards remembered should subtract 1 as weight(A) = 15
            int memoryBenefit = optimal is null
                ? -1
                : MaxCardWeight - CardService.GetCardWeight(optimal);

            utilities.Add(decision, individualCardWeight + memoryBenefit);
            _logger.LogInformation($@"
                {LoggerPrefix} Registered possible decision '{decision.Strategy}' on '{decision.Card}' with 
                individual card weight '{individualCardWeight}' and memory benefit '{memoryBenefit}'
                ");
        }

        // TryOthersTake strategy would depend on suit-count in hand, hence cannot be computed here
    }

    private (Decision<InitialThrowStrategy>, int) AddEliminateSuitDecision(ReadOnlySpan<Card> suit)
    {
        // ordered by face ascending, first should be lowest
        Card lowestCard = suit[0];

        // will never be 14, always prefer other strats if they are guaranteed to work out
        int utility = MaxUtility - suit.Length;

        Decision<InitialThrowStrategy> decision = new(lowestCard, InitialThrowStrategy.TryOthersTake);
        _logger.LogInformation($@"
                {LoggerPrefix} Registered possible decision '{decision.Strategy}' on '{decision.Card}' with 
                utility '{utility}'
                ");
        return (decision, utility);
    }

    private Dictionary<Decision<InitialThrowStrategy>, int> GenerateDecisions(
        List<Card> hand, List<Card> memory)
    {
        IOrderedEnumerable<Card> orderedHand = hand.OrderBy(x => x.Suit)
            .ThenBy(x => x.Face);
        _logger.LogInformation($"{LoggerPrefix} Ordered hand: {orderedHand.Stringify()}; Memory: {memory.Stringify()}");

        Dictionary<Decision<InitialThrowStrategy>, int> decisions = new();

        foreach (Card card in hand)
        {
            AddUtilitiesOverCard(decisions, card, memory);
        }

        _cardUtilities.SegmentIntoSpans(
            orderedHand,
            out _, // discard hearts, not being calculated
            out var diamonds,
            out var clover,
            out var spades);

        if (diamonds.Length > 0)
        {
            (var decision, var utility) = AddEliminateSuitDecision(diamonds);
            decisions.Add(decision, utility);
        }

        if (spades.Length > 0)
        {
            (var decision, var utility) = AddEliminateSuitDecision(spades);
            decisions.Add(decision, utility);
        }

        if (clover.Length > 0)
        {
            (var decision, var utility) = AddEliminateSuitDecision(clover);
            decisions.Add(decision, utility);
        }

        return decisions;
    }

    public Card MakeDecision(List<Card> hand, List<Card> memory)
    {
        var decisions = GenerateDecisions(hand, memory);
        var optimal = DecisionFilterUtilities.FilterOptimal(decisions);
        _logger.LogInformation($"{LoggerPrefix} Optimal decisions are: '{optimal.Stringify()}'");

        int index = optimal.FindIndex(x => x.Strategy == InitialThrowStrategy.TryTake);
        if (index > -1)
        {
            _logger.LogInformation($"{LoggerPrefix} Try-take available, throwing: '{optimal[index].Card}'");
            return optimal[index].Card;
        }

        index = optimal.FindIndex(x => x.Strategy == InitialThrowStrategy.LowHeart);
        if (index > -1)
        {
            _logger.LogInformation(
                $"{LoggerPrefix} No Try-take exists; Low Heart available, throwing: '{optimal[index].Card}'");
            return optimal[index].Card;
        }

        index = optimal.FindIndex(x => x.Strategy == InitialThrowStrategy.TryOthersTake);
        if (index > -1)
        {
            _logger.LogInformation(
                $"{LoggerPrefix} Only Try-Others-Take available, throwing: '{optimal[index].Card}'");
            return optimal[index].Card;
        }

        throw new UnreachableException("No optimal decision matches filters.");
    }
}
