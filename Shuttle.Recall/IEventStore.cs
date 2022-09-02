using System;

namespace Shuttle.Recall
{
    public interface IEventStore
    {
        EventStream CreateEventStream(Guid id);
        EventStream CreateEventStream();
        EventStream Get(Guid id);
        long Save(EventStream eventStream);
        long Save(EventStream eventStream, Action<EventEnvelopeBuilder> builder);
        void Remove(Guid id);
    }
}