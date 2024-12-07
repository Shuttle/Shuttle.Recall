using System.Threading.Tasks;

namespace Shuttle.Recall;

public interface IProjectionService
{
    Task<ProjectionEvent?> GetProjectionEventAsync();
    Task SetSequenceNumberAsync(string projectionName, long sequenceNumber);
    ValueTask<long> GetSequenceNumberAsync(string projectionName);
}