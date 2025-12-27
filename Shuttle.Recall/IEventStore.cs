namespace Shuttle.Recall;

public interface IEventStore
{
    Task<EventStream> GetAsync(Guid id, Action<EventStreamBuilder>? builder = null, CancellationToken cancellationToken = default);
    Task RemoveAsync(Guid id, CancellationToken cancellationToken = default);
    Task SaveAsync(EventStream eventStream, Action<EventStreamBuilder>? builder = null, CancellationToken cancellationToken = default);
}