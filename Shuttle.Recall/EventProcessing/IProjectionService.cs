using System.Threading.Tasks;

namespace Shuttle.Recall;

public interface IProjectionService
{
    Task<ProjectionEvent?> GetProjectionEventAsync(int processorThreadManagedThreadId);
    Task SetSequenceNumberAsync(string projectionName, long sequenceNumber);
}