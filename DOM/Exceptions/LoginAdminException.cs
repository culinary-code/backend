namespace DOM.Exceptions;

public class LoginAdminException : Exception
{
    public LoginAdminException()
    {
    }

    public LoginAdminException(string? message) : base(message)
    {
    }

    public LoginAdminException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}