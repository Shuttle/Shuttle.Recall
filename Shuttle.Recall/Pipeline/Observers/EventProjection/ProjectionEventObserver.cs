using Shuttle.Contract;
using Shuttle.Pipelines;

namespace Shuttle.Recall;

public interface IProjectionEventObserver : IPipelineObserver<RetrieveEvent>;

public class ProjectionEventObserver(IProjectionEventService projectionEventService) : IProjectionEventObserver
{
    private readonly IProjectionEventService _projectionEventService = Guard.AgainstNull(projectionEventService);

    public async Task ExecuteAsync(IPipelineContext<RetrieveEvent> pipelineContext, CancellationToken cancellationToken = default)
    {
        var state = Guard.AgainstNull(pipelineContext).Pipeline.State;
        var projectionEvent = await _projectionEventService.RetrieveAsync(pipelineContext, cancellationToken).ConfigureAwait(false);

        if (projectionEvent == null)
        {
            pipelineContext.Pipeline.Abort();

            return;
        }

        state.SetWorkPerformed();
        state.SetProjectionEvent(projectionEvent);
    }
}