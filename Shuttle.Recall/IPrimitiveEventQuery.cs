namespace Shuttle.Recall;

public interface IPrimitiveEventQuery
{
    Task<IEnumerable<PrimitiveEvent>> SearchAsync(PrimitiveEvent.Specification specification, CancellationToken cancellationToken = default);
    Task<long?> GetMaximumSequenceNumberAsync(PrimitiveEvent.Specification specification, CancellationToken cancellationToken = default);
}