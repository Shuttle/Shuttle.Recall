using System.Threading.Tasks;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Recall;

public interface IAcknowledgeEventObserver : IPipelineObserver<OnAcknowledgeEvent>
{
}

public class AcknowledgeEventObserver : IAcknowledgeEventObserver
{
    private readonly IProjectionService _service;

    public AcknowledgeEventObserver(IProjectionService projectionService)
    {
        _service = Guard.AgainstNull(projectionService);
    }

    public async Task ExecuteAsync(IPipelineContext<OnAcknowledgeEvent> pipelineContext)
    {
        await _service.AcknowledgeEventAsync(Guard.AgainstNull(pipelineContext)).ConfigureAwait(false);
    }
}