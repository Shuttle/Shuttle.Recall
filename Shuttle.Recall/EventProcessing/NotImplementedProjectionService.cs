using Shuttle.Core.Pipelines;

namespace Shuttle.Recall;

public class NotImplementedProjectionService : IProjectionService
{
    public Task<ProjectionEvent?> GetEventAsync(IPipelineContext<RetrieveEvent> pipelineContext, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException(Resources.NotImplementedProjectionEventProvider);
    }

    public Task AcknowledgeEventAsync(IPipelineContext<AcknowledgeEvent> pipelineContext, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException(Resources.NotImplementedProjectionEventProvider);
    }
}