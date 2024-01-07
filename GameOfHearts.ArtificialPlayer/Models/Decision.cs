using GameOfHearts.Game.Models;

namespace GameOfHearts.ArtificialPlayer.Models;

public sealed class Decision<TStrategy> where TStrategy : Enum
{
    public Card Card { get; set; }
    public TStrategy Strategy { get; set; }

    public Decision(Card card, TStrategy strategy)
    {
        Card = card;
        Strategy = strategy;
    }

    public override string ToString()
    {
        return $"Decision with Strategy: '{Strategy}' on Card '{Card}";
    }
}
