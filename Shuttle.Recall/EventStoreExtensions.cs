using System;

namespace Shuttle.Recall
{
    public static class EventStoreExtensions
    {
        public static EventStream Get(this IEventStore eventStore)
        {
            return eventStore.Get(Guid.Empty);
        }
    }
}