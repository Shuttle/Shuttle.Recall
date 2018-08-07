using System;
using Shuttle.Core.Contract;

namespace Shuttle.Recall
{
    public static class EventStreamExtensions
    {
        public static T Get<T>(this EventStream stream) where T : class
        {
            Guard.AgainstNull(stream, nameof(stream));

            stream.EmptyInvariant();

            T result;

            try
            {
                result = (T)Activator.CreateInstance(typeof(T), stream.Id);
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

        public static void EmptyInvariant(this EventStream stream)
        {
            if (stream == null || stream.IsEmpty)
            {
                throw new EventStreamEmptyException();
            }
        }
    }
}