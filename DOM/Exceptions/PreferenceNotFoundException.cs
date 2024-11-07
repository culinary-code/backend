using System;

namespace DOM.Exceptions;

public class PreferenceNotFoundException : Exception
{
    public PreferenceNotFoundException()
    {
    }
    
    public PreferenceNotFoundException(string message) : base(message)
    {
    }
    
    public PreferenceNotFoundException(string message, Exception innerException) : base(message, innerException)
    {
    }
}