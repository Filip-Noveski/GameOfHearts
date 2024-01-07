namespace GameOfHearts.ArtificialPlayer.Enums;

/// <summary>
/// Defines the strategies used during the later throws, 
/// given the player has the thrown suit.
/// </summary>
public enum LaterThrowSameSuitStrategy
{
    /// <summary>
    /// Throw the highest possible card, aiming to take.
    /// Higher chances of taking should increase utility.
    /// Prefered when goal is to eliminate certain suit that has 
    /// not been thrown i.e., player aims to take in order to throw
    /// specific card next time round.
    /// </summary>
    TryTake,

    /// <summary>
    /// <i>**Default when initial suit is Hearts.</i>
    /// Throw the highest possible card, but lower to avoid taking.
    /// Utility increased by higher cards with lower chance of taking.
    /// Prefered when player wants to eliminate higher cards, but does
    /// not want to take at the given time.
    /// </summary>
    AvoidTake
    //TryOthersTakeWithHighest,

    // Non-viable; ommitted
    ///// <summary>
    ///// Throw a lower card aiming to keep higher cards to try take later.
    ///// Utility increased by lower cards.
    ///// Prefered when goal is to eliminate certain suit that has 
    ///// not been thrown, but cannot take at this time i.e., player 
    ///// aims to take when possible in order to throw specific card later on.
    ///// </summary>
    //TryOthersTakeWithLower
}
