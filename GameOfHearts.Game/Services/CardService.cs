using GameOfHearts.Game.Enums;
using GameOfHearts.Game.Models;
using System.Diagnostics;

namespace GameOfHearts.Game.Services;

public static class CardService
{
    private static readonly char HeartChar = '\u2665';
    private static readonly char DiamondChar = '\u2666';
    private static readonly char CloverChar = '\u2663';
    private static readonly char SpadeChar = '\u2660';

    public static string Stringify(Card card)
    {
        return $"{Stringify(card.Face)} {Stringify(card.Suit)}";
    }

    public static string Stringify(CardSuit suit)
    {
        return suit switch
        {
            CardSuit.Spade => SpadeChar.ToString(),
            CardSuit.Diamond => DiamondChar.ToString(),
            CardSuit.Heart => HeartChar.ToString(),
            CardSuit.Clover => CloverChar.ToString(),
            _ => throw new UnreachableException($"Tried to parse card suit int: '{(int)suit}'")
        };
    }

    public static string Stringify(CardFace face)
    {
        return face switch
        {
            CardFace.Ace => "A",
            CardFace.Two => "2",
            CardFace.Three => "3",
            CardFace.Four => "4",
            CardFace.Five => "5",
            CardFace.Six => "6",
            CardFace.Seven => "7",
            CardFace.Eight => "8",
            CardFace.Nine => "9",
            CardFace.Ten => "10",
            CardFace.Jack => "J",
            CardFace.Queen => "Q",
            CardFace.King => "K",
            _ => throw new UnreachableException($"Tried to parse card face int: '{(int)face}'")
        };
    }

    public static Card InstantiateCard(CardSuit suit, CardFace face)
    {
        if (suit is not CardSuit.Heart)
        {
            return new Card(suit, face);
        }

        int value = face switch
        {
            CardFace.Ace => 5,
            CardFace.King => 4,
            CardFace.Queen => 3,
            CardFace.Jack => 2,
            _ => 1
        };

        return new Card(suit, face, value);
    }

    public static int GetCardWeight(Card card)
    {
        return card.Face switch
        {
            CardFace.Two => 2,
            CardFace.Three => 3,
            CardFace.Four => 4,
            CardFace.Five => 5,
            CardFace.Six => 6,
            CardFace.Seven => 7,
            CardFace.Eight => 8,
            CardFace.Nine => 9,
            CardFace.Ten => 10,
            CardFace.Jack => 12,
            CardFace.Queen => 13,
            CardFace.King => 14,
            CardFace.Ace => 15,
            _ => throw new UnreachableException($"Unkown card face int: {(int)card.Face}")
        };
    }

    public static int GetTotalScore(IEnumerable<Card> cards)
    {
        int score = 0;

        foreach (Card card in cards)
        {
            score += card.Value;
        }

        return score;
    }
}
