namespace Shuttle.Recall;

public interface IPrimitiveEventRepository
{
    Task<IEnumerable<PrimitiveEvent>> GetAsync(Guid id, CancellationToken cancellationToken = default);
    ValueTask<long> GetMaxSequenceNumberAsync(CancellationToken cancellationToken = default);
    ValueTask<long> GetSequenceNumberAsync(Guid id, CancellationToken cancellationToken = default);
    Task RemoveAsync(Guid id, CancellationToken cancellationToken = default);
    ValueTask<long> SaveAsync(IEnumerable<PrimitiveEvent> primitiveEvents, CancellationToken cancellationToken = default);
}