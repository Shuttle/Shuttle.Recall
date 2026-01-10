using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Recall;

public interface IAcknowledgeEventObserver : IPipelineObserver<AcknowledgeEvent>;

public class AcknowledgeEventObserver(IProjectionEventService projectionEventService) : IAcknowledgeEventObserver
{
    private readonly IProjectionEventService _eventService = Guard.AgainstNull(projectionEventService);

    public async Task ExecuteAsync(IPipelineContext<AcknowledgeEvent> pipelineContext, CancellationToken cancellationToken = default)
    {
        await _eventService.AcknowledgeAsync(Guard.AgainstNull(pipelineContext), cancellationToken).ConfigureAwait(false);
    }
}