using System;

namespace Shuttle.Recall
{
    public interface IEventStore
    {
        EventStream GetEventStream(Guid id);
        EventStream GetEventStreamAll(Guid id);
        void RemoveEventStream(Guid id);
        void SaveEventStream(EventStream eventStream);
        void SaveEventStream(EventStream eventStream, Action<EventEnvelopeConfigurator> configure);
    }
}