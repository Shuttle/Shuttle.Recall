using Shuttle.Core.Contract;
using Shuttle.Core.Threading;

namespace Shuttle.Recall
{
    public class EventHandlerContext<T> : IEventHandlerContext<T> where T : class
    {
        public EventHandlerContext(EventEnvelope eventEnvelope, T @event, PrimitiveEvent primitiveEvent,
            IThreadState activeState)
        {
            Guard.AgainstNull(eventEnvelope, nameof(eventEnvelope));
            Guard.AgainstNull(@event, nameof(@event));
            Guard.AgainstNull(primitiveEvent, nameof(primitiveEvent));
            Guard.AgainstNull(activeState, nameof(activeState));

            ActiveState = activeState;
            Event = @event;
            PrimitiveEvent = primitiveEvent;
            EventEnvelope = eventEnvelope;
        }

        public EventEnvelope EventEnvelope { get; }
        public PrimitiveEvent PrimitiveEvent { get; }
        public T Event { get; }
        public IThreadState ActiveState { get; }
    }
}