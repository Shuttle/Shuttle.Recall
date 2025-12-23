using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Recall;

public interface IAcknowledgeEventObserver : IPipelineObserver<AcknowledgeEvent>;

public class AcknowledgeEventObserver(IProjectionService projectionService) : IAcknowledgeEventObserver
{
    private readonly IProjectionService _service = Guard.AgainstNull(projectionService);

    public async Task ExecuteAsync(IPipelineContext<AcknowledgeEvent> pipelineContext, CancellationToken cancellationToken = default)
    {
        await _service.AcknowledgeEventAsync(Guard.AgainstNull(pipelineContext), cancellationToken).ConfigureAwait(false);
    }
}