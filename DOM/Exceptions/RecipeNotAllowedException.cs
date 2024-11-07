namespace DOM.Exceptions;

public class RecipeNotAllowedException : Exception
{
    public string ReasonMessage { get; set; }
    
    public RecipeNotAllowedException(string reasonMessage)
    {
        ReasonMessage = reasonMessage;
    }
    
    public RecipeNotAllowedException(string message, string reasonMessage) : base(message)
    {
        ReasonMessage = reasonMessage;
    }
    
    public RecipeNotAllowedException(string message, Exception innerException, string reasonMessage) : base(message, innerException)
    {
        ReasonMessage = reasonMessage;
    }
}