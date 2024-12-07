using System;

namespace Shuttle.Recall;

public class AggregateConstructorException : Exception
{
    public AggregateConstructorException()
    {
    }

    public AggregateConstructorException(string message) : base(message)
    {
    }

    public AggregateConstructorException(string message, Exception innerException) : base(message, innerException)
    {
    }
}