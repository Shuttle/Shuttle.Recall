using System;

namespace Shuttle.Recall;

public class UnhandledEventException : Exception
{
    public UnhandledEventException()
    {
    }

    public UnhandledEventException(string message) : base(message)
    {
    }

    public UnhandledEventException(string message, Exception innerException) : base(message, innerException)
    {
    }
}