namespace Shuttle.Recall;

public interface IProjectionRepository
{
    Task<Projection> GetAsync(string name, CancellationToken cancellationToken = default);
    Task SaveAsync(Projection projection, CancellationToken cancellationToken = default);
}