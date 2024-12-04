namespace DOM.Exceptions;

public class ItemQuantityNotFoundException : Exception
{
    public ItemQuantityNotFoundException()
    {
    }

    public ItemQuantityNotFoundException(string? message) : base(message)
    {
    }

    public ItemQuantityNotFoundException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}