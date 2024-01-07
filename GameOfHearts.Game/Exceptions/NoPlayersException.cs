namespace GameOfHearts.Game.Exceptions;

public class NoPlayersException : Exception
{
    public NoPlayersException() : base()
    {

    }

    public NoPlayersException(string? message) : base(message)
    {

    }

    public NoPlayersException(string? message, Exception? innerException) : base(message, innerException)
    {

    }
}
