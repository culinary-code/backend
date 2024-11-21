namespace DOM.Exceptions;

public class GroceryListItemNotFoundException : Exception
{
    public GroceryListItemNotFoundException()
    {
    }

    public GroceryListItemNotFoundException(string? message) : base(message)
    {
    }

    public GroceryListItemNotFoundException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}