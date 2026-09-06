namespace Shuttle.Recall;

public interface IEventTypeRepository
{
    Task<Guid> GetIdAsync(string typeName, CancellationToken cancellationToken = default);
}