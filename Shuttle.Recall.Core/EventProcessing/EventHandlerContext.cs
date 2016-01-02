using Shuttle.Core.Infrastructure;

namespace Shuttle.Recall.Core
{
    public class EventHandlerContext<T> : IEventHandlerContext<T> where T : class
    {
        public EventHandlerContext(IEventProcessorConfiguration configuration,  ProjectionEvent projectionEvent, T domainEvent, IThreadState activeState)
        {
	        Configuration = configuration;
	        ActiveState = activeState;
            DomainEvent = domainEvent;
            ProjectionEvent = projectionEvent;
        }

        public ProjectionEvent ProjectionEvent { get; private set; }
        public T DomainEvent { get; private set; }
	    public IEventProcessorConfiguration Configuration { get; private set; }
	    public IThreadState ActiveState { get; private set; }
    }
}