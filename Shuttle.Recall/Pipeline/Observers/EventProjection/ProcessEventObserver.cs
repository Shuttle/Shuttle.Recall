using System.Threading.Tasks;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Recall;

public interface IProcessEventObserver : IPipelineObserver<OnProcessEvent>
{
}

public class ProcessEventObserver : IProcessEventObserver
{
    public async Task ExecuteAsync(IPipelineContext<OnProcessEvent> pipelineContext)
    {
        var state = Guard.AgainstNull(pipelineContext).Pipeline.State;
        var projectionEvent = state.GetProjectionEvent();

        if (!projectionEvent.HasPrimitiveEvent)
        {
            return;
        }

        await state.GetProjection().ProcessAsync(state.GetEventEnvelope(), state.GetEvent().Event, projectionEvent.PrimitiveEvent!, pipelineContext.Pipeline.CancellationToken);
    }
}