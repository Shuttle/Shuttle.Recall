using System;
using System.Threading.Tasks;

namespace Shuttle.Recall;

public interface IEventStore
{
    Task<EventStream> GetAsync(Guid id, Action<EventStreamBuilder>? builder = null);
    Task RemoveAsync(Guid id);
    ValueTask<long> SaveAsync(EventStream eventStream, Action<EventStreamBuilder>? builder = null);
}