using Shuttle.Core.Pipelines;

namespace Shuttle.Recall;

public interface IProjectionService
{
    Task AcknowledgeEventAsync(IPipelineContext<AcknowledgeEvent> pipelineContext, CancellationToken cancellationToken = default);
    Task<ProjectionEvent?> RetrieveEventAsync(IPipelineContext<RetrieveEvent> pipelineContext, CancellationToken cancellationToken = default);
}