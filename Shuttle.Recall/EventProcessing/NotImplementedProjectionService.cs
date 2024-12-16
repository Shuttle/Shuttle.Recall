using System;
using System.Threading.Tasks;

namespace Shuttle.Recall;

public class NotImplementedProjectionService : IProjectionService
{
    public Task<ProjectionEvent?> GetProjectionEventAsync(int processorThreadManagedThreadId)
    {
        throw new NotImplementedException(Resources.NotImplementedProjectionEventProvider);
    }

    public Task AcknowledgeAsync(ProjectionEvent projectionEvent)
    {
        throw new NotImplementedException(Resources.NotImplementedProjectionEventProvider);
    }
}