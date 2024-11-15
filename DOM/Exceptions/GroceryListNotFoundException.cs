using System.Runtime.Serialization;

namespace DOM.Exceptions;

public class GroceryListNotFoundException : Exception
{
    public GroceryListNotFoundException()
    {
    }

    protected GroceryListNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public GroceryListNotFoundException(string? message) : base(message)
    {
    }

    public GroceryListNotFoundException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}