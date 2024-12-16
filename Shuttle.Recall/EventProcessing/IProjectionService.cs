using System.Threading.Tasks;

namespace Shuttle.Recall;

public interface IProjectionService
{
    Task<ProjectionEvent?> GetProjectionEventAsync(int processorThreadManagedThreadId);
    Task AcknowledgeAsync(ProjectionEvent projectionEvent);
}