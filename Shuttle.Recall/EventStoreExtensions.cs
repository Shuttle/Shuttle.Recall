using Shuttle.Core.Contract;

namespace Shuttle.Recall;

public static class EventStoreExtensions
{
    extension(IEventStore eventStore)
    {
        public async Task<EventStream> GetAsync(Action<EventStreamBuilder>? builder = null, CancellationToken cancellationToken = default)
        {
            return await Guard.AgainstNull(eventStore).GetAsync(Guid.Empty, builder, cancellationToken);
        }

        public async Task<EventStream> GetAsync(CancellationToken cancellationToken = default)
        {
            return await Guard.AgainstNull(eventStore).GetAsync(Guid.Empty, null, cancellationToken);
        }

        public async Task<EventStream> GetAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await Guard.AgainstNull(eventStore).GetAsync(id, null, cancellationToken);
        }

        public async Task<IEnumerable<EventEnvelope>> SaveAsync(EventStream eventStream, CancellationToken cancellationToken = default)
        {
            return await Guard.AgainstNull(eventStore).SaveAsync(eventStream, null, cancellationToken);
        }
    }
}