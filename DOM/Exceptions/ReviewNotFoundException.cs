namespace DOM.Exceptions;

public class ReviewNotFoundException : Exception
{
    public ReviewNotFoundException()
    {
    }
    
    public ReviewNotFoundException(string message) : base(message)
    {
    }
    
    public ReviewNotFoundException(string message, Exception innerException) : base(message, innerException)
    {
    }
    
}