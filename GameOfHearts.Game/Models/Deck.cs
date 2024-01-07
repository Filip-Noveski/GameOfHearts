using GameOfHearts.Game.Enums;
using GameOfHearts.Game.Exceptions;
using GameOfHearts.Game.Services;

namespace GameOfHearts.Game.Models;

public sealed class Deck
{
    private static readonly Random RNG = new();

    private readonly List<Card> _cards;

    public Deck()
    {
        _cards = new(52);
        // Loop over all suits
        foreach (CardSuit suit in Enum.GetValues(typeof(CardSuit)))
        {
            // Loop over all faces
            foreach (CardFace face in Enum.GetValues(typeof(CardFace)))
            {
                // Instantiate combination with valid value from CardService
                Card card = CardService.InstantiateCard(suit, face);
                _cards.Add(card);
            }
        }
    }

    /// <summary>
    /// Shuffles the deck randomly.
    /// </summary>
    public void Shuffle()
    {
        int n = _cards.Count;
        while (n > 1)
        {
            n--;
            int k = RNG.Next(n + 1);
            (_cards[n], _cards[k]) = (_cards[k], _cards[n]);
        }
    }

    /// <summary>
    /// Gets and returns the first card, removing it from the Deck,
    /// or throws if it is empty.
    /// </summary>
    /// 
    /// <returns>
    /// The card at the top of the deck.
    /// </returns>
    /// 
    /// <exception cref="DeckEmptyException">
    /// Thrown if the deck is empty i.e., has no cards.
    /// </exception>
    public Card GetCard()
    {
        if (_cards.Count == 0)
        {
            throw new DeckEmptyException("The deck was empty");
        }

        Card card = _cards[0];
        _cards.RemoveAt(0);

        return card;
    }

    public bool IsEmpty()
    {
        return _cards.Count == 0;
    }
}
