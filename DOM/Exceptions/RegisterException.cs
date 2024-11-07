using System;

namespace DOM.Exceptions;

public class RegisterUserException : Exception
{
    public RegisterUserException()
    {
    }

    public RegisterUserException(string? message) : base(message)
    {
    }

    public RegisterUserException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}