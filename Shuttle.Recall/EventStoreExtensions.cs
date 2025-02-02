using System;
using System.Threading.Tasks;

namespace Shuttle.Recall;

public static class EventStoreExtensions
{
    public static async Task<EventStream> GetAsync(this IEventStore eventStore, Action<EventStreamBuilder>? builder = null)
    {
        return await eventStore.GetAsync(Guid.Empty, builder);
    }
}