using System.Runtime.Serialization;

namespace DOM.Exceptions;

public class MealPlannerNotFoundException : Exception
{
    public MealPlannerNotFoundException()
    {
    }

    public MealPlannerNotFoundException(string? message) : base(message)
    {
    }

    public MealPlannerNotFoundException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}