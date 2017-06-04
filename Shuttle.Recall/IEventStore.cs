using System;

namespace Shuttle.Recall
{
    public interface IEventStore
    {
        EventStream CreateEventStream(Guid id);
        EventStream CreateEventStream();
        EventStream Get(Guid id);
        void Save(EventStream eventStream);
        void Save(EventStream eventStream, Action<EventEnvelopeConfigurator> configurator);
        void Remove(Guid id);
    }
}