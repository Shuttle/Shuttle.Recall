using System.Threading.Tasks;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Recall;

public interface IProjectionEventObserver : IPipelineObserver<OnGetProjectionEvent>
{
}

public class ProjectionEventObserver : IProjectionEventObserver
{
    private readonly IProjectionEventProvider _provider;

    public ProjectionEventObserver(IProjectionEventProvider provider)
    {
        _provider = Guard.AgainstNull(provider);
    }

    public async Task ExecuteAsync(IPipelineContext<OnGetProjectionEvent> pipelineContext)
    {
        var state = Guard.AgainstNull(pipelineContext).Pipeline.State;
        var projection = state.GetProjection();
        var projectionEvent = await _provider.GetAsync(projection);

        if (projectionEvent.HasPrimitiveEvent)
        {
            state.SetWorking();
        }

        state.SetProjectionEvent(projectionEvent);
    }
}