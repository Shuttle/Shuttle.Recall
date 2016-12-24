using Shuttle.Core.Infrastructure;

namespace Shuttle.Recall
{
    public class EventHandlerContext<T> : IEventHandlerContext<T> where T : class
    {
        public EventHandlerContext(EventEnvelope eventEnvelope, T @event, long sequenceNumber, IThreadState activeState)
        {
	        ActiveState = activeState;
            Event = @event;
            SequenceNumber = sequenceNumber;
            EventEnvelope = eventEnvelope;
        }

        public EventEnvelope EventEnvelope { get; private set; }
        public T Event { get; private set; }
        public long SequenceNumber { get; private set; }
        public IThreadState ActiveState { get; private set; }
    }
}