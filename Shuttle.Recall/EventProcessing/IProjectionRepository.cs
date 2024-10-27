using System.Threading.Tasks;

namespace Shuttle.Recall;

public interface IProjectionRepository
{
    Task<Projection?> FindAsync(string name);
    ValueTask<long> GetSequenceNumberAsync(string projectionName);
    Task SaveAsync(Projection projection);
    Task SetSequenceNumberAsync(string projectionName, long sequenceNumber);
}