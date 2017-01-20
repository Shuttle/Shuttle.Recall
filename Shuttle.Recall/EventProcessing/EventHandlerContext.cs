using Shuttle.Core.Infrastructure;

namespace Shuttle.Recall
{
    public class EventHandlerContext<T> : IEventHandlerContext<T> where T : class
    {
        public EventHandlerContext(EventEnvelope eventEnvelope, T @event, PrimitiveEvent primitiveEvent, IThreadState activeState)
        {
            Guard.AgainstNull(eventEnvelope, "eventEnvelope");
            Guard.AgainstNull(@event, "@event");
            Guard.AgainstNull(primitiveEvent, "primitiveEvent");
            Guard.AgainstNull(activeState, "activeState");

            ActiveState = activeState;
            Event = @event;
            PrimitiveEvent = primitiveEvent;
            EventEnvelope = eventEnvelope;
        }

        public EventEnvelope EventEnvelope { get; private set; }
        public PrimitiveEvent PrimitiveEvent { get; private set; }
        public T Event { get; private set; }
        public IThreadState ActiveState { get; private set; }
    }
}