using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Recall;

public interface IProjectionEventObserver : IPipelineObserver<RetrieveEvent>;

public class ProjectionEventObserver(IProjectionService projectionService) : IProjectionEventObserver
{
    private readonly IProjectionService _projectionService = Guard.AgainstNull(projectionService);

    public async Task ExecuteAsync(IPipelineContext<RetrieveEvent> pipelineContext, CancellationToken cancellationToken = default)
    {
        var state = Guard.AgainstNull(pipelineContext).Pipeline.State;
        var projectionEvent = await _projectionService.RetrieveEventAsync(pipelineContext, cancellationToken).ConfigureAwait(false);

        if (projectionEvent == null)
        {
            pipelineContext.Pipeline.Abort();

            return;
        }

        state.SetWorkPerformed();
        state.SetProjectionEvent(projectionEvent);
    }
}