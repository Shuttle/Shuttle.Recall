using System.Threading.Tasks;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Recall;

public interface IProjectionEventObserver : IPipelineObserver<OnGetEvent>
{
}

public class ProjectionEventObserver : IProjectionEventObserver
{
    private readonly IProjectionService _provider;

    public ProjectionEventObserver(IProjectionService provider)
    {
        _provider = Guard.AgainstNull(provider);
    }

    public async Task ExecuteAsync(IPipelineContext<OnGetEvent> pipelineContext)
    {
        var state = Guard.AgainstNull(pipelineContext).Pipeline.State;
        var projectionEvent = await _provider.GetEventAsync(pipelineContext).ConfigureAwait(false);

        if (projectionEvent == null)
        {
            pipelineContext.Pipeline.Abort();

            return;
        }

        state.SetWorking();
        state.SetProjectionEvent(projectionEvent);
    }
}