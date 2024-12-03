namespace DOM.Exceptions;

public class IngredientQuantityNotFoundException : Exception
{
    public IngredientQuantityNotFoundException()
    {
    }

    public IngredientQuantityNotFoundException(string? message) : base(message)
    {
    }

    public IngredientQuantityNotFoundException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}