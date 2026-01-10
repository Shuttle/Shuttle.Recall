using Shuttle.Core.Pipelines;

namespace Shuttle.Recall;

public class NotImplementedProjectionEventService : IProjectionEventService
{
    public Task<ProjectionEvent?> RetrieveAsync(IPipelineContext<RetrieveEvent> pipelineContext, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException(Resources.NotImplementedProjectionEventProvider);
    }

    public async Task PipelineFailedAsync(IPipelineContext<PipelineFailed> pipelineContext, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException(Resources.NotImplementedProjectionEventProvider);
    }

    public Task AcknowledgeAsync(IPipelineContext<AcknowledgeEvent> pipelineContext, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException(Resources.NotImplementedProjectionEventProvider);
    }
}