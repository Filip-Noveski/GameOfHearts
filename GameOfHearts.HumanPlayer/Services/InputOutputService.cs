using GameOfHearts.Game.Models;
using System.Net.Http.Headers;

namespace GameOfHearts.HumanPlayer.Services;

public sealed class InputOutputService
{
    private static readonly ConsoleColor ViableColour = ConsoleColor.Cyan;
    private static readonly ConsoleColor DefaultColour = ConsoleColor.Gray;

    private readonly List<Card> _viableCards;

    public InputOutputService()
    {
        _viableCards = new();
    }

    private static void PrintViable(int selectIndex, Card card, bool printInitialPipe = true)
    {
        Console.ForegroundColor = ViableColour;
        if (printInitialPipe)
        {
            Console.Write("|");
        }
        Console.Write($" {selectIndex}. ");

        Console.ForegroundColor = DefaultColour;
        card.Print();

        Console.ForegroundColor = ViableColour;
        Console.Write($" |");

        Console.ForegroundColor = DefaultColour;
    }

    private static void PrintNonViable(Card card, bool printInitialPipe = true)
    {
        if (printInitialPipe)
        {
            Console.Write("| ");
        }
        else
        {
            Console.Write(' ');
        }
        card.Print();
        Console.Write(' ');
    }

    /// <summary>
    /// Prints all the cards in the hand, considering the ones mapped to <code>true</code>
    /// to be viable.
    /// </summary>
    /// 
    /// <param name="hand">
    /// Key-value pairs where the key is the card to print, and
    /// the value is a boolean representing whether it is viable.
    /// </param>
    public void PrintHandAndRegisterViable(Dictionary<Card, bool> hand)
    {
        bool printInitialPipe = true;
        int selectIndex = 1;
        foreach (var pair in hand)
        {
            if (pair.Value)
            {
                _viableCards.Add(pair.Key);
                PrintViable(selectIndex++, pair.Key, printInitialPipe);
                printInitialPipe = false;
                continue;
            }

            PrintNonViable(pair.Key, printInitialPipe);
            printInitialPipe = true;
        }

        Console.Write("\nChoose card to throw: ");
    }

    /// <summary>
    /// Prints all the cards in the hand and considers all viable.
    /// </summary>
    /// <param name="hand">The cards to print.</param>
    public void PrintHandAndRegisterViable(List<Card> hand)
    {
        for (int i = 0; i <= hand.Count - 1; i++)
        {
            _viableCards.Add(hand[i]);
            PrintViable(i + 1, hand[i], i == 0);    // print initial pipe on first print
        }

        Console.Write("\nChoose card to throw: ");
    }

    public Card ReadInputAndClearViable()
    {
        int index;

    Retry:
        bool parsed = int.TryParse(Console.ReadLine(), out index);
        if (!parsed)
        {
            // make sure user enters a number
            goto Retry;
        }
        if (index > _viableCards.Count || index < 1)
        {
            // make sure number is within bounds
            goto Retry;
        }

        Card thrown = _viableCards[index - 1];
        _viableCards.Clear();
        return thrown;
    }
}
