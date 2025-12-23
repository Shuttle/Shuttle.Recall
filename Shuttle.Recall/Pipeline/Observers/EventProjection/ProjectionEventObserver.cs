using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Recall;

public interface IProjectionEventObserver : IPipelineObserver<RetrieveEvent>;

public class ProjectionEventObserver(IProjectionService provider) : IProjectionEventObserver
{
    private readonly IProjectionService _provider = Guard.AgainstNull(provider);

    public async Task ExecuteAsync(IPipelineContext<RetrieveEvent> pipelineContext, CancellationToken cancellationToken = default)
    {
        var state = Guard.AgainstNull(pipelineContext).Pipeline.State;
        var projectionEvent = await _provider.GetEventAsync(pipelineContext, cancellationToken).ConfigureAwait(false);

        if (projectionEvent == null)
        {
            pipelineContext.Pipeline.Abort();

            return;
        }

        state.SetWorking();
        state.SetProjectionEvent(projectionEvent);
    }
}