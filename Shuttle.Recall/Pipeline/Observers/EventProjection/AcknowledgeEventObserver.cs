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
        var state = Guard.AgainstNull(pipelineContext).Pipeline.State;
        var projectionEvent = Guard.AgainstNull(state.GetProjectionEvent());

        await _service.SetSequenceNumberAsync(projectionEvent.Projection.Name, projectionEvent.Projection.SequenceNumber).ConfigureAwait(false);
    }
}