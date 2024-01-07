using GameOfHearts.ArtificialPlayer.Utilities;
using GameOfHearts.Game.Enums;
using GameOfHearts.Game.Models;
using GameOfHearts.Game.Services;
using GameOfHearts.LoggingProvider.Extensions;
using GameOfHearts.LoggingProvider.Services;
using System.Diagnostics;

namespace GameOfHearts.ArtificialPlayer.Services;

public sealed class LaterThrowSameSuitService
{
    private static readonly string LoggerPrefix = "[later throw, same suit]";

    private readonly CardsUtilities _cardUtilities;
    private readonly Logger _logger;

    public LaterThrowSameSuitService(Logger logger)
    {
        _logger = logger;
        _cardUtilities = new();
    }

    private ReadOnlySpan<Card> GetThrowableCards(List<Card> hand, CardSuit suit)
    {
        IOrderedEnumerable<Card> orderedHand = hand.OrderBy(x => x.Suit)
            .ThenBy(x => x.Face);

        _cardUtilities.SegmentIntoSpans(
            orderedHand,
            out var hearts,
            out var diamonds,
            out var clover,
            out var spades);

        return suit switch
        {
            CardSuit.Heart => hearts,
            CardSuit.Diamond => diamonds,
            CardSuit.Clover => clover,
            CardSuit.Spade => spades,
            _ => throw new UnreachableException($"Unknown suit int: {suit}")
        };
    }

    private bool IsTryTakeViable(ReadOnlySpan<Card> throwable, ReadOnlySpan<Card> trick)
    {
        int maxThrowableWeight = _cardUtilities.GetMaxCardWeight(throwable);
        int maxTrickWeight = _cardUtilities.GetMaxCardWeight(trick);

        // has cards higher than current highest
        return maxThrowableWeight - maxTrickWeight > 0;
    }
    
    private bool IsAvoidTakeViable(ReadOnlySpan<Card> throwable, ReadOnlySpan<Card> trick)
    {
        int maxThrowableWeight = _cardUtilities.GetMaxCardWeight(throwable);
        int maxTrickWeight = _cardUtilities.GetMaxCardWeight(trick);

        return maxThrowableWeight - maxTrickWeight < 0;
    }

    private static int GetTakeWeight(CardSuit initialSuit, ReadOnlySpan<Card> trick)
    {
        int? weight = null;
        foreach (Card card in trick)
        {
            if (weight is null && card.Suit == initialSuit)
            {
                weight = CardService.GetCardWeight(card);
                continue;
            }

            int currentWeight = CardService.GetCardWeight(card);
            if (weight < currentWeight && card.Suit == initialSuit)
            {
                weight = currentWeight;
            }
        }
        return weight ?? -1;
    }

    private static int GetTotalHeartsValue(ReadOnlySpan<Card> cards)
    {
        int score = 0;
        foreach (Card card in cards)
        {
            score += card.Value;
        }
        return score;
    }

    private static int GetTotalHeartsInHand(List<Card> cards)
    {
        int count = 0;
        foreach (Card card in cards)
        {
            if (card.Suit is CardSuit.Heart)
            {
                count++;
            }
        }
        return count;
    }

    private static Card FindHighestCardBelow(int weight, ReadOnlySpan<Card> cards)
    {
        Card card = cards[0];
        int selectedWeight = CardService.GetCardWeight(card);
        for (int i = 1; i <= cards.Length - 1; i++)
        {
            int currentWeight = CardService.GetCardWeight(cards[i]);
            if (currentWeight < weight && currentWeight > selectedWeight)
            {
                card = cards[i];
                selectedWeight = currentWeight;
            }
        }

        return card;
    }

    private Card MakeDecisionBothViable(
        List<Card> hand, 
        ReadOnlySpan<Card> throwable, 
        CardSuit suit,
        ReadOnlySpan<Card> trick)
    {
        IOrderedEnumerable<Card> orderedHand = hand.OrderBy(x => x.Suit)
            .ThenBy(x => x.Face);

        _cardUtilities.SegmentIntoSpans(
            orderedHand,
            out var hearts,
            out var diamonds,
            out var clover,
            out var spades);

        int minNonHeartSuitCount = Math.Min(diamonds.Length, Math.Min(clover.Length, spades.Length));
        int currentTakeWeight = GetTakeWeight(suit, trick);

        bool earlyGame = hand.Count > 8;
        bool lateGame = hand.Count < 6;
        // there is a suit with few cards if it reperesents less than 20% of all cards in hand
        bool lowCardCountSuit = minNonHeartSuitCount / (float)hand.Count < 0.2f;
        int totalHeartsValueInTrick = GetTotalHeartsValue(trick);
        int totalHeartsInHand = GetTotalHeartsInHand(hand);

        if (!earlyGame && totalHeartsInHand / (float)hand.Count > 0.5f)
        {
            // too many hearts, risky to take, avoid
            Card card = FindHighestCardBelow(currentTakeWeight, throwable);
            _logger.LogInformation(
                $"{LoggerPrefix} Not early game and more than 50% hearts in hand, avoiding take with: '{card}'");
            return card;
        }

        if (earlyGame && totalHeartsValueInTrick == 0)
        {
            // early on, throw highest to take; then decide how to play
            Card card = throwable[^1];
            _logger.LogInformation(
                $"{LoggerPrefix} Early game and no hearts in trick, trying take with: '{card}'");
            return card;
        }

        if (!lateGame && lowCardCountSuit && totalHeartsValueInTrick == 0)
        {
            // early enough - risk should be low, take to eliminate other suit
            Card card = throwable[^1];
            _logger.LogInformation($@"
                {LoggerPrefix} Not late game, has suit with few cards and 
                no hearts in trick, trying take with: '{card}'
                ");
            return card;
        }

        if (earlyGame && lowCardCountSuit && totalHeartsValueInTrick <= 1)
        {
            // early game, suit with few cards, worth taking
            Card card = throwable[^1];
            _logger.LogInformation($@"
                {LoggerPrefix} Early game, has suit with few cards and 
                less than 1 in hearts value in trick, trying take with: '{card}'
                ");
            return card;
        }

        // otherwise, avoid take
        Card cardEnd = FindHighestCardBelow(currentTakeWeight, throwable);
        _logger.LogInformation($"{LoggerPrefix} No pre-conditions met, avoiding take with: '{cardEnd}'");
        return cardEnd;
    }

    public Card MakeDecision(List<Card> hand, CardSuit suit, ReadOnlySpan<Card> trick)
    {
        ReadOnlySpan<Card> throwable = GetThrowableCards(hand, suit);
        int currentTakeWeight = GetTakeWeight(suit, trick);
        _logger.LogInformation($@"
            {LoggerPrefix} Hand: {hand.Stringify()}; 
            throwable: {throwable.Stringify()}; 
            take weight: {currentTakeWeight}
            ");

        if (suit is CardSuit.Heart)
        {
            Card card = FindHighestCardBelow(currentTakeWeight, throwable);
            _logger.LogInformation($"{LoggerPrefix} Suit is Hearts, throwing highest possible, but lower: '{card}'");
            // throw lowest available heart
            return card;
        }

        if (throwable.Length == 1)
        {
            Card card = throwable[0];
            _logger.LogInformation($"{LoggerPrefix} Can only throw one: '{card}'");
            return card;
        }

        if (!IsTryTakeViable(throwable, trick) || !IsAvoidTakeViable(throwable, trick))
        {
            Card card = throwable[^1];
            _logger.LogInformation($"{LoggerPrefix} Only one strategy viable, throwing highest: '{card}'");
            // last is highest, throw it; always optimal
            return card; 
        }

        return MakeDecisionBothViable(hand, throwable, suit, trick);
    }
}
