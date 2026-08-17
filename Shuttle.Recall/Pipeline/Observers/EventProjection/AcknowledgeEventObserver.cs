using Shuttle.Contract;
using Shuttle.Pipelines;

namespace Shuttle.Recall;

public interface IAcknowledgeEventObserver : IPipelineObserver<AcknowledgeEvent>;

public class AcknowledgeEventObserver(IProjectionEventService projectionEventService) : IAcknowledgeEventObserver
{
    public async Task ExecuteAsync(IPipelineContext<AcknowledgeEvent> pipelineContext, CancellationToken cancellationToken = default)
    {
        await projectionEventService.AcknowledgeAsync(Guard.AgainstNull(pipelineContext), cancellationToken).ConfigureAwait(false);
    }
}