using System.Threading.Tasks;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Recall;

public interface IAcknowledgeEventObserver : IPipelineObserver<OnAcknowledgeEvent>
{
}

public class AcknowledgeEventObserver : IAcknowledgeEventObserver
{
    private readonly IProjectionRepository _repository;

    public AcknowledgeEventObserver(IProjectionRepository repository)
    {
        _repository = Guard.AgainstNull(repository);
    }

    public async Task ExecuteAsync(IPipelineContext<OnAcknowledgeEvent> pipelineContext)
    {
        var state = Guard.AgainstNull(pipelineContext).Pipeline.State;
        var projection = state.GetProjection();
        var projectionEvent = Guard.AgainstNull(state.GetProjectionEvent());

        if (!projectionEvent.HasPrimitiveEvent)
        {
            return;
        }

        await _repository.SetSequenceNumberAsync(projection.Name, projection.SequenceNumber).ConfigureAwait(false);
    }
}