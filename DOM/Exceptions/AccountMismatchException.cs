namespace DOM.Exceptions;

public class AccountMismatchException : Exception
{
    public AccountMismatchException()
    {
    }

    public AccountMismatchException(string? message) : base(message)
    {
    }

    public AccountMismatchException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}