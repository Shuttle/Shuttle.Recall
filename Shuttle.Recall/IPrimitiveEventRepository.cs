namespace Shuttle.Recall;

public interface IPrimitiveEventRepository
{
    Task<IEnumerable<PrimitiveEvent>> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task RemoveAsync(PrimitiveEvent.Specification specification, CancellationToken cancellationToken = default);
    Task SaveAsync(IEnumerable<PrimitiveEvent> primitiveEvents, CancellationToken cancellationToken = default);
}