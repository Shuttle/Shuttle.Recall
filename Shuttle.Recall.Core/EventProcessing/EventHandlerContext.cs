using Shuttle.Core.Infrastructure;

namespace Shuttle.Recall.Core
{
    public class EventHandlerContext<T> : IEventHandlerContext<T> where T : class
    {
        public EventHandlerContext(EventRead eventRead, T domainEvent, IThreadState activeState)
        {
            ActiveState = activeState;
            DomainEvent = domainEvent;
            EventRead = eventRead;
        }

        public EventRead EventRead { get; private set; }
        public T DomainEvent { get; private set; }
        public IThreadState ActiveState { get; private set; }
    }
}