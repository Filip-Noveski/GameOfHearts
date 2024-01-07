using GameOfHearts.Game.Enums;
using GameOfHearts.Game.Models;
using GameOfHearts.Game.Services;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace GameOfHearts.ArtificialPlayer.Utilities;

public sealed class CardsUtilities
{
    private static void GetSuitIndexes(List<Card> cards, out Dictionary<CardSuit, int> indexes)
    {
        int heart = cards.FindIndex(x => x.Suit == CardSuit.Heart);
        int diamond = cards.FindIndex(x => x.Suit == CardSuit.Diamond);
        int clover = cards.FindIndex(x => x.Suit == CardSuit.Clover);
        int spade = cards.FindIndex(x => x.Suit == CardSuit.Spade);

        indexes = new()
        {
            { CardSuit.Heart, heart },
            { CardSuit.Spade, spade },
            { CardSuit.Clover, clover },
            { CardSuit.Diamond, diamond }
        };

        indexes = indexes.OrderBy(x => x.Value).ToDictionary();
    }

    /// <summary>
    /// Segments the hand's cards based on suit, returning
    /// <see cref="ReadOnlySpan{T}"/>'s pointing to the original list.
    /// </summary>
    /// <param name="hand">The hand ordered by suit.</param>
    /// <param name="hearts">A <see cref="ReadOnlySpan{T}"/> pointing to the Heart-suit cards.</param>
    /// <param name="diamonds">A <see cref="ReadOnlySpan{T}"/> pointing to the Diamond-suit cards.</param>
    /// <param name="clover">A <see cref="ReadOnlySpan{T}"/> pointing to the Clover-suit cards.</param>
    /// <param name="spades">A <see cref="ReadOnlySpan{T}"/> pointing to the Spade-suit cards.</param>
#pragma warning disable CA1822 // Mark members as static
    public void SegmentIntoSpans(
        IOrderedEnumerable<Card> hand, 
        out ReadOnlySpan<Card> hearts,
        out ReadOnlySpan<Card> diamonds,
        out ReadOnlySpan<Card> clover,
        out ReadOnlySpan<Card> spades)
    {
#pragma warning disable IDE0305 // Simplify collection initialization
        List<Card> handAsList = hand.ToList();
#pragma warning restore IDE0305 // Simplify collection initialization
        ReadOnlySpan<Card> handAsSpan = CollectionsMarshal.AsSpan(handAsList);

        GetSuitIndexes(handAsList, out var indexes);

        hearts = ReadOnlySpan<Card>.Empty;
        diamonds = ReadOnlySpan<Card>.Empty;
        spades = ReadOnlySpan<Card>.Empty;
        clover = ReadOnlySpan<Card>.Empty;

        // 0 -> initial suit; 3 -> last suit of 4 total
        for (CardSuit i = 0; i <= (CardSuit)3; i++)
        {
            int start = indexes[i];
            int end = indexes.Values
                .Order()
                .ToList()
                .Find(x => x > start);

            if (start == -1)
            { 
                continue; 
            }

            if (end is default(int))
            {
                end = handAsSpan.Length;
            }

            _ = i switch
            {
                CardSuit.Heart => hearts = handAsSpan[start..end],
                CardSuit.Diamond => diamonds = handAsSpan[start..end],
                CardSuit.Spade => spades = handAsSpan[start..end],
                CardSuit.Clover => clover = handAsSpan[start..end],
                _ => throw new UnreachableException($"Unknown suit int: {i}")
            };
        }
    }

    /// <summary>
    /// Returns the highest face card starting from 2 without a jump, or null if 2 is absent.
    /// e.g., int the sequence 2, 3, 4, 6...; 4 is highest, as 5 is skipped.
    /// </summary>
    /// <param name="cards">The cards to search over.</param>
    /// <returns>The discored result or null.</returns>
    public Card? GetHighestCardNonBreakingFrom2(List<Card> cards)
    {
        Card? final = null;
        Card[] ordered = cards.OrderBy(x => x.Face).ToArray();
        for (int i = 0; i <= ordered.Length - 1; i++)
        {
            if (ordered[i].Face != (CardFace)i)
                break;

            final = ordered[i];
        }

        return final;
    }

    /// <summary>
    /// Returns the lowest face card starting from Ace without a jump, or null if Ace is absent.
    /// e.g., int the sequence A, K, Q, 10...; Q is highest, as J is skipped.
    /// </summary>
    /// <param name="cards">The cards to search over.</param>
    /// <returns>The discored result or null.</returns>
    public Card? GetLowestCardNonBreakingFromAce(List<Card> cards)
    {
        Card? final = null;
        Card[] ordered = cards.OrderByDescending(x => x.Face).ToArray();
        for (int i = ordered.Length - 1; i >= 0; i--)
        {
            if (ordered[i].Face != CardFace.Ace - i)
                break;

            final = ordered[i];
        }

        return final;
    }

    public int GetMaxCardWeight(ReadOnlySpan<Card> cards)
    {
        int maxWeight = CardService.GetCardWeight(cards[0]);
        for (int i = 1; i <= cards.Length - 1; i++)
        {
            int currentWeight = CardService.GetCardWeight(cards[i]);
            if (maxWeight < currentWeight)
            {
                maxWeight = currentWeight;
            }
        }

        return maxWeight;
    }
}
