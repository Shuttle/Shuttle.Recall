using System;

namespace Shuttle.Recall
{
    public interface IEventStore
    {
        EventStream GetEventStream(Guid id);
        void SaveEventStream(EventStream eventStream);
        void SaveEventStream(EventStream eventStream, Action<EventEnvelopeConfigurator> configurator);
    }
}