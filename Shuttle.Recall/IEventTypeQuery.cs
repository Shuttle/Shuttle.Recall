namespace Shuttle.Recall;

public interface IEventTypeQuery
{
    Task<IEnumerable<Query.EventType>> SearchAsync(Query.EventType.Specification specification, CancellationToken cancellationToken = default);
}