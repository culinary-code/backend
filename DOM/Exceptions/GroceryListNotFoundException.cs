using System.Runtime.Serialization;

namespace DOM.Exceptions;

public class GroceryListNotFoundException : Exception
{
    public GroceryListNotFoundException()
    {
    }

    public GroceryListNotFoundException(string? message) : base(message)
    {
    }

    public GroceryListNotFoundException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}