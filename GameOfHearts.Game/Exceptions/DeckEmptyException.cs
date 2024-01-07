namespace GameOfHearts.Game.Exceptions;

public class DeckEmptyException : Exception
{
    public DeckEmptyException() : base()
    {
        
    }

    public DeckEmptyException(string? message) : base(message)
    {
        
    }

    public DeckEmptyException(string? message, Exception? innerException) : base(message, innerException)
    {
        
    }
}
