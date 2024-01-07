using GameOfHearts.Game.Models;

namespace GameOfHearts.Game.Interfaces;

/// <summary>
/// Player interface.
/// </summary>
public interface IPlayer
{
    /// <summary>
    /// Display name of the player, used in UI.
    /// </summary>
    string Name { get; init; }

    /// <summary>
    /// The player's score calculated as the sum of the values 
    /// of the Heart-suit cards taken during all the games.
    /// </summary>
    int Score { get; set; }

    /// <summary>
    /// Adds the provided card to the player's hand.
    /// </summary>
    /// <param name="card">The card to add.</param>
    /// <returns></returns>
    Task GrabDealtCard(Card card);

    /// <summary>
    /// Throws the initial card for the current trick.
    /// </summary>
    /// <returns>The card that the player has decided to throw.</returns>
    Task<Card> ThrowCard();

    /// <summary>
    /// Throws a later card during the current trick.
    /// </summary>
    /// <param name="initial">The initial card.</param>
    /// <param name="trick">The current trick (all cards thrown).</param>
    /// <returns>The card that the player has decided to throw.</returns>
    Task<Card> ThrowCard(Card initial, ReadOnlySpan<Card> trick);

    /// <summary>
    /// Returns an <see cref="IEnumerable{T}"/> of <see cref="Card"/>
    /// containing all cards taken during this game.
    /// </summary>
    /// <returns>
    /// An <see cref="IEnumerable{T}"/> of <see cref="Card"/>
    /// containing all cards taken during this game.
    /// </returns>
    IEnumerable<Card> GetTakenCards();

    /// <summary>
    /// Checks and returns whether the player has any cards in his/her hand.
    /// </summary>
    /// <returns>
    /// <code>true</code> if the player has cards;
    /// <code>false</code> otherwise.
    /// </returns>
    bool HasCards();

    /// <summary>
    /// Adds the <see cref="IEnumerable{T}"/> of <see cref="Card"/>
    /// to the list of taken cards during this game.
    /// </summary>
    /// <param name="cards">The cards to be added.</param>
    /// <returns></returns>
    Task TakeTrick(IEnumerable<Card> cards);

    /// <summary>
    /// Removes all <see cref="Card"/> entries in the list of taken cards.
    /// </summary>
    void ClearTakenCards();

    /// <summary>
    /// <b>AI Players Only. For Human Players, leave empty.</b>
    /// Adds each of the cards to the player's memory with a certain probability of remembering.
    /// </summary>
    /// <param name="trick">The cards thrown during this trick.</param>
    Task TryRemember(Dictionary<int, Card> trick);
}
