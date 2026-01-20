using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Recall;

public static class PipelineStateExtensions
{
    extension(IState state)
    {
        public DateTimeOffset? GetDeferredUntil()
        {
            return state.Get<DateTimeOffset?>(StateKeys.DeferredUntil);
        }

        public DomainEvent GetDomainEvent()
        {
            return Guard.AgainstNull(state.Get<DomainEvent>(StateKeys.DomainEvent));
        }

        public byte[] GetEventBytes()
        {
            return Guard.AgainstNull(state.Get<byte[]>(StateKeys.EventBytes));
        }

        public EventEnvelope GetEventEnvelope()
        {
            return Guard.AgainstNull(state.Get<EventEnvelope>(StateKeys.EventEnvelope));
        }

        public IEnumerable<EventEnvelope>? GetEventEnvelopes()
        {
            return state.Get<IEnumerable<EventEnvelope>>(StateKeys.EventEnvelopes);
        }

        public IEnumerable<DomainEvent> GetEvents()
        {
            return Guard.AgainstNull(state.Get<IEnumerable<DomainEvent>>(StateKeys.Events));
        }

        public EventStream GetEventStream()
        {
            return Guard.AgainstNull(state.Get<EventStream>(StateKeys.EventStream));
        }

        public EventStreamBuilder GetEventStreamBuilder()
        {
            return Guard.AgainstNull(state.Get<EventStreamBuilder>(StateKeys.EventStreamBuilder));
        }

        public Guid GetId()
        {
            return state.Get<Guid>(StateKeys.Id);
        }

        public PrimitiveEvent GetPrimitiveEvent()
        {
            return Guard.AgainstNull(state.Get<PrimitiveEvent>(StateKeys.PrimitiveEvent));
        }

        public int GetProcessorThreadManagedThreadId()
        {
            return state.Get<int>(StateKeys.ProcessorThreadManagedThreadId);
        }

        public ProjectionEvent GetProjectionEvent()
        {
            return Guard.AgainstNull(state.Get<ProjectionEvent>(StateKeys.ProjectionEvent));
        }

        public int GetVersion()
        {
            return state.Get<int>(StateKeys.Version);
        }

        public bool GetWorkPerformed()
        {
            return state.Contains(StateKeys.WorkPerformed) && state.Get<bool>(StateKeys.WorkPerformed);
        }

        public void SetDeferredUntil(DateTimeOffset deferredUntil)
        {
            state.Replace(StateKeys.DeferredUntil, deferredUntil);
        }
        
        public void SetDomainEvent(DomainEvent domainEvent)
        {
            state.Replace(StateKeys.DomainEvent, domainEvent);
        }

        public void SetEventBytes(byte[] bytes)
        {
            state.Replace(StateKeys.EventBytes, Guard.AgainstNull(bytes));
        }

        public void SetEventEnvelope(EventEnvelope value)
        {
            state.Replace(StateKeys.EventEnvelope, value);
        }

        public void SetEventEnvelopes(IEnumerable<EventEnvelope> value)
        {
            state.Replace(StateKeys.EventEnvelopes, value);
        }

        public void SetEvents(IEnumerable<DomainEvent> value)
        {
            state.Replace(StateKeys.Events, value);
        }

        public void SetEventStream(EventStream value)
        {
            state.Replace(StateKeys.EventStream, value);
        }

        public void SetEventStreamBuilder(EventStreamBuilder value)
        {
            state.Replace(StateKeys.EventStreamBuilder, value);
        }

        public void SetId(Guid id)
        {
            state.Replace(StateKeys.Id, id);
        }

        public void SetPrimitiveEvent(PrimitiveEvent? primitiveEvent)
        {
            state.Replace(StateKeys.PrimitiveEvent, primitiveEvent);
        }

        public void SetProcessorThreadManagedThreadId(int processorThreadManagedThreadId)
        {
            state.Replace(StateKeys.ProcessorThreadManagedThreadId, processorThreadManagedThreadId);
        }

        public void SetProjectionEvent(ProjectionEvent projectionEvent)
        {
            state.Replace(StateKeys.ProjectionEvent, projectionEvent);
        }

        public void SetVersion(int value)
        {
            state.Replace(StateKeys.Version, value);
        }

        public void SetWorkPerformed()
        {
            state.Replace(StateKeys.WorkPerformed, true);
        }
    }
}