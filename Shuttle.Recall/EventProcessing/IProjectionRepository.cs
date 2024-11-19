using System.Threading.Tasks;

namespace Shuttle.Recall;

public interface IProjectionRepository
{
    Task<Projection?> FindAsync(string projectionName);
    ValueTask<long> GetSequenceNumberAsync(string projectionName);
    Task SaveAsync(Projection projection);
    Task SetSequenceNumberAsync(string projectionName, long sequenceNumber);
}