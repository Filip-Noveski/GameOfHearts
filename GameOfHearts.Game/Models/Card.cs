using GameOfHearts.Game.Enums;
using GameOfHearts.Game.Services;

namespace GameOfHearts.Game.Models;

public sealed class Card
{
    public CardSuit Suit { get; init; }

    public CardFace Face { get; init; }

    public int Value { get; init; }

    public Card(CardSuit suit, CardFace face, int value)
    {
        if (suit != CardSuit.Heart && value != 0)
        {
            Console.Error.WriteLine($"""
                Card of Suit "{suit}" instantiated with Value "{value}", which should not be allowed.
                Did you mean to set the suit to "{CardSuit.Heart}" or Value to "0"?
                """);
        }

        Suit = suit;
        Face = face;
        Value = value;
    }

    public Card(CardSuit suit, CardFace face)
    {
        Suit = suit;
        Face = face;
        Value = 0;
    }

    public override string ToString()
    {
        return CardService.Stringify(this);
    }

    public void Print()
    {
        string text = this.ToString();
        if (Suit is CardSuit.Heart or CardSuit.Diamond)
        {
            Console.ForegroundColor = ConsoleColor.Red;
        }
        Console.Write(text);

        Console.ForegroundColor = ConsoleColor.Gray;
    }
}
