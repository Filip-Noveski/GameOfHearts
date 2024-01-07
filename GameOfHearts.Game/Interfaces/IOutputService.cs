using GameOfHearts.Game.Models;

namespace GameOfHearts.Game.Interfaces;

public interface IOutputService
{
    void DisplayTrick(ReadOnlySpan<Card> trick);

    void DisplayTaker(string takerName);
}
