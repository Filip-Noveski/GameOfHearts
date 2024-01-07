namespace GameOfHearts.ArtificialPlayer.Enums;

/// <summary>
/// Defines the strategies used during the initial throw.
/// </summary>
public enum InitialThrowStrategy
{
    /// <summary>
    /// Throw a low Heart card, aiming for other players to take.
    /// Lower chances of taking should increase utility.
    /// Prefered when no Aces of others suits available.
    /// </summary>
    LowHeart,

    /// <summary>
    /// Throw a high non-hearts card (preferably Ace) aiming to take.
    /// Higher chances of taking increase utility.
    /// Prefered when Aces (or other high cards depending on progression)
    /// available and on suits with fewer cards.
    /// </summary>
    TryTake,

    /// <summary>
    /// Throw a low non-hearts card aiming for others to take.
    /// Lower chances of taking increase utility.
    /// Prefered when no card guarantees taking and on a suit
    /// that has few cards, ideally one i.e., try to eliminate suit.
    /// </summary>
    TryOthersTake
}
