using Shuttle.Contract;
using Shuttle.Pipelines;

namespace Shuttle.Recall;

public interface IEventProcessingPipelineFailedObserver : IPipelineObserver<PipelineFailed>;

public class EventProcessingPipelineFailedObserver(IProjectionEventService projectionEventService) : IEventProcessingPipelineFailedObserver
{
    public async Task ExecuteAsync(IPipelineContext<PipelineFailed> pipelineContext, CancellationToken cancellationToken = default)
    {
        await projectionEventService.PipelineFailedAsync(Guard.AgainstNull(pipelineContext), cancellationToken).ConfigureAwait(false);
    }
}