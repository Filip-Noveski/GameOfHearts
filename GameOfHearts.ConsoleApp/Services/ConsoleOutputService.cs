using GameOfHearts.Game.Interfaces;
using GameOfHearts.Game.Models;

namespace GameOfHearts.ConsoleApp.Services;

public sealed class ConsoleOutputService : IOutputService
{
    public void DisplayTaker(string takerName)
    {
        Console.WriteLine($"Player {takerName} is taking trick");
    }

    public void DisplayTrick(ReadOnlySpan<Card> trick)
    {
        Console.Write("Trick: ");
        foreach (Card card in trick)
        {
            Console.Write(" | ");
            card.Print();
        }
        Console.Write("\n\n\n");
    }
}
