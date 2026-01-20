using Shuttle.Core.Pipelines;

namespace Shuttle.Recall;

public interface IProjectionEventService
{
    Task AcknowledgeAsync(IPipelineContext<AcknowledgeEvent> pipelineContext, CancellationToken cancellationToken = default);
    Task<ProjectionEvent?> RetrieveAsync(IPipelineContext<RetrieveEvent> pipelineContext, CancellationToken cancellationToken = default);
    Task DeferAsync(IPipelineContext<HandleEvent> pipelineContext, CancellationToken cancellationToken = default);
    Task PipelineFailedAsync(IPipelineContext<PipelineFailed> pipelineContext, CancellationToken cancellationToken = default);
}