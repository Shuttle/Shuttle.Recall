using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Recall;

public interface IEventProcessingPipelineFailedObserver : IPipelineObserver<PipelineFailed>;

public class EventProcessingPipelineFailedObserver(IProjectionEventService projectionEventService) : IEventProcessingPipelineFailedObserver
{
    private readonly IProjectionEventService _eventService = Guard.AgainstNull(projectionEventService);

    public async Task ExecuteAsync(IPipelineContext<PipelineFailed> pipelineContext, CancellationToken cancellationToken = default)
    {
        await _eventService.PipelineFailedAsync(Guard.AgainstNull(pipelineContext), cancellationToken).ConfigureAwait(false);
    }
}