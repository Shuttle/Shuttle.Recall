using System;
using System.Threading.Tasks;
using Shuttle.Core.Pipelines;

namespace Shuttle.Recall;

public class NotImplementedProjectionService : IProjectionService
{
    public Task<ProjectionEvent?> GetEventAsync(IPipelineContext<OnGetEvent> pipelineContext)
    {
        throw new NotImplementedException(Resources.NotImplementedProjectionEventProvider);
    }

    public Task AcknowledgeEventAsync(IPipelineContext<OnAcknowledgeEvent> pipelineContext)
    {
        throw new NotImplementedException(Resources.NotImplementedProjectionEventProvider);
    }
}