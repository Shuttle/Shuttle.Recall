using System.Threading;
using Shuttle.Core.Contract;

namespace Shuttle.Recall
{
    public class EventHandlerContext<T> : IEventHandlerContext<T> where T : class
    {
        public EventHandlerContext(EventEnvelope eventEnvelope, T @event, PrimitiveEvent primitiveEvent, CancellationToken cancellationToken)
        {
            Guard.AgainstNull(eventEnvelope, nameof(eventEnvelope));
            Guard.AgainstNull(@event, nameof(@event));
            Guard.AgainstNull(primitiveEvent, nameof(primitiveEvent));

            CancellationToken = cancellationToken;
            Event = @event;
            PrimitiveEvent = primitiveEvent;
            EventEnvelope = eventEnvelope;
        }

        public EventEnvelope EventEnvelope { get; }
        public PrimitiveEvent PrimitiveEvent { get; }
        public T Event { get; }
        public CancellationToken CancellationToken { get; }
    }
}