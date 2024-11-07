namespace DOM.Exceptions;

public class RecipeValidationFailException : Exception
{
    public RecipeValidationFailException()
    {
    }
    
    public RecipeValidationFailException(string message) : base(message)
    {
    }
    
    public RecipeValidationFailException(string message, Exception innerException) : base(message, innerException)
    {
    }
}