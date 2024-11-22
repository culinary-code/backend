namespace DOM.Exceptions;

public class ReviewAlreadyExistsException : Exception
{
    public ReviewAlreadyExistsException()
    {
    }
    
    public ReviewAlreadyExistsException(string message) : base(message)
    {
    }
    
    public ReviewAlreadyExistsException(string message, Exception innerException) : base(message, innerException)
    {
    }
}