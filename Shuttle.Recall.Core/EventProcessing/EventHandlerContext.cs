using Shuttle.Core.Infrastructure;

namespace Shuttle.Recall.Core
{
    public class EventHandlerContext<T> : IEventHandlerContext<T> where T : class
    {
        public EventHandlerContext(ProjectionEvent projectionEvent, T domainEvent, IThreadState activeState)
        {
            ActiveState = activeState;
            DomainEvent = domainEvent;
            ProjectionEvent = projectionEvent;
        }

        public ProjectionEvent ProjectionEvent { get; private set; }
        public T DomainEvent { get; private set; }
        public IThreadState ActiveState { get; private set; }
    }
}