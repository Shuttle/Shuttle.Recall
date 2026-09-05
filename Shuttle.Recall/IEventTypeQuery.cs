namespace Shuttle.Recall;

public interface IEventTypeQuery
{
    Task<IEnumerable<EventType>> SearchAsync(PrimitiveEvent.Specification specification, CancellationToken cancellationToken = default);
}