using System;

namespace DOM.Exceptions;

public class EnvironmentVariableNotAvailableException : Exception
{
    public EnvironmentVariableNotAvailableException()
    {
    }
    
    public EnvironmentVariableNotAvailableException(string message) : base(message)
    {
    }
    
    public EnvironmentVariableNotAvailableException(string message, Exception innerException) : base(message, innerException)
    {
    }
}