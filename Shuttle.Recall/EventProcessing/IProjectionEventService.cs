using Shuttle.Pipelines;

namespace Shuttle.Recall;

public interface IProjectionEventService
{
    Task AcknowledgeAsync(IPipelineContext<AcknowledgeEvent> pipelineContext, CancellationToken cancellationToken = default);
    Task<ProjectionEvent?> RetrieveAsync(IPipelineContext<RetrieveEvent> pipelineContext, CancellationToken cancellationToken = default);
    Task DeferAsync(IPipelineContext<HandleEvent> pipelineContext, CancellationToken cancellationToken = default);
    Task PipelineFailedAsync(IPipelineContext<PipelineFailed> pipelineContext, CancellationToken cancellationToken = default);

    /// <summary>
    /// Records that <paramref name="projectionName"/> has already handled the event identified by
    /// <paramref name="eventId"/>, via immediate consistency. The eventual event processor uses this to skip
    /// re-invoking the handler for that event while still advancing the projection's checkpoint across it.
    /// </summary>
    Task ProjectionEventHandledAsync(string projectionName, Guid eventId, CancellationToken cancellationToken = default);
}