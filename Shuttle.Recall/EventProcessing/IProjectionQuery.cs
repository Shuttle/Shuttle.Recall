namespace Shuttle.Recall;

public interface IProjectionQuery
{
    ValueTask<Query.Projection> SearchAsync(Query.Projection.Specification specification, CancellationToken cancellationToken = default);
    ValueTask<Query.Projection?> GetPendingAsync(CancellationToken cancellationToken = default);
    ValueTask<bool> HasPendingProjectionsAsync(long sequenceNumber, CancellationToken cancellationToken = default);
}