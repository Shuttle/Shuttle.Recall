namespace Shuttle.Recall;

public interface IPrimitiveEventQuery
{
    Task<IEnumerable<Query.PrimitiveEvent>> SearchAsync(Query.PrimitiveEvent.Specification specification, CancellationToken cancellationToken = default);
    Task<long?> GetMaximumSequenceNumberAsync(Query.PrimitiveEvent.Specification specification, CancellationToken cancellationToken = default);
}