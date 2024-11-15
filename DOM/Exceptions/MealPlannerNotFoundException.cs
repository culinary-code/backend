using System.Runtime.Serialization;

namespace DOM.Exceptions;

public class MealPlannerNotFoundException : Exception
{
    public MealPlannerNotFoundException()
    {
    }

    protected MealPlannerNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public MealPlannerNotFoundException(string? message) : base(message)
    {
    }

    public MealPlannerNotFoundException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}