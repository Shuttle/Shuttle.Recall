using Shuttle.Contract;

namespace Shuttle.Recall;

public static class EventStreamExtensions
{
    extension(EventStream stream)
    {
        public EventStream MustHaveEvents()
        {
            ArgumentNullException.ThrowIfNull(stream);

            return stream.IsEmpty ? throw new EventStreamException(string.Format(Resources.EventStreamEmptyException, stream.Id)) : stream;
        }

        public EventStream MustBeEmpty()
        {
            ArgumentNullException.ThrowIfNull(stream);

            return !stream.IsEmpty ? throw new EventStreamException(string.Format(Resources.EventStreamNotEmptyException, stream.Id)) : stream;
        }

        public T Get<T>() where T : class
        {
            ArgumentNullException.ThrowIfNull(stream);

            T result;

            try
            {
                var type = typeof(T);

                if (type.GetConstructor([typeof(Guid)]) != null)
                {
                    result = Guard.AgainstNull(Activator.CreateInstance(type, stream.Id) as T);
                }
                else if (type.GetConstructor(Type.EmptyTypes) != null)
                {
                    result = Guard.AgainstNull(Activator.CreateInstance(type) as T);
                }
                else
                {
                    throw new AggregateConstructorException(Resources.AggregateConstructorException);
                }
            }
            catch (AggregateConstructorException)
            {
                throw;
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
}