namespace GameOfHearts.ArtificialPlayer.Enums;

/// <summary>
/// Defines the strategies used during the later throws, 
/// given the player does not have the thrown suit.
/// </summary>
public enum LaterThrowOtherSuitStrategy
{
    /// <summary>
    /// <i>**Default if the player has any Hearts.</i>
    /// Throw the highest available heart.
    /// Higher utility for higher face value.
    /// Prefered anytime the player has hearts.
    /// </summary>
    ThrowHighestHeart,

    /// <summary>
    /// <i>**Only select if the player does not have hearts.</i>
    /// Throw the highest card of the suit with fewest cards.
    /// Higher utlity for suits with fewer cards.
    /// Prefered when the player has a suit with few cards, ideally one.
    /// </summary>
    EliminateSuitWithFewCards,

    /// <summary>
    /// <i>**Only select if the player does not have hearts.</i>
    /// Throw the highest available card.
    /// Higher utility for higher cards, and of suits with fewer cards
    /// if tie for highest card.
    /// Prefered when the player has relatively even number of cards
    /// across suits.
    /// </summary>
    EliminateHighCards
}
