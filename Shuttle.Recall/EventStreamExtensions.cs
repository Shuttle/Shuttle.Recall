using System;
using Shuttle.Core.Contract;

namespace Shuttle.Recall;

public static class EventStreamExtensions
{
    public static void EmptyInvariant(this EventStream stream)
    {
        if (stream == null || stream.IsEmpty)
        {
            throw new EventStreamEmptyException();
        }
    }

    public static T Get<T>(this EventStream stream) where T : class
    {
        Guard.AgainstNull(stream).EmptyInvariant();

        T result;

        try
        {
            result = Guard.AgainstNull(Activator.CreateInstance(typeof(T), stream.Id) as T);
        }
        catch (Exception ex)
        {
            throw new AggregateConstructorException(Resources.AggregateConstructorException, ex);
        }

        if (!stream.IsEmpty)
        {
            stream.Apply(result);
        }

        return result;
    }
}