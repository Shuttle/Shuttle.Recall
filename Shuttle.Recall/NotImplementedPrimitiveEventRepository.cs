namespace Shuttle.Recall;

public class NotImplementedPrimitiveEventRepository : IPrimitiveEventRepository
{
    public Task RemoveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException(Resources.NotImplementedPrimitiveEventRepository);
    }

    public Task<IEnumerable<PrimitiveEvent>> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException(Resources.NotImplementedPrimitiveEventRepository);
    }

    public ValueTask<long> SaveAsync(IEnumerable<PrimitiveEvent> primitiveEvents, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException(Resources.NotImplementedPrimitiveEventRepository);
    }

    public ValueTask<long> GetMaxSequenceNumberAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException(Resources.NotImplementedPrimitiveEventRepository);
    }

    public ValueTask<long> GetSequenceNumberAsync(Guid id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException(Resources.NotImplementedPrimitiveEventRepository);
    }
}