using Microsoft.Extensions.Options;
using Shuttle.Contract;
using Shuttle.Pipelines;

namespace Shuttle.Recall;

public interface IHandleEventObserver : IPipelineObserver<HandleEvent>;

public class HandleEventObserver(IOptions<RecallOptions> recallOptions, IEventHandlerInvoker eventMethodInvoker, IProjectionEventService projectionEventService) : IHandleEventObserver
{
    public async Task ExecuteAsync(IPipelineContext<HandleEvent> pipelineContext, CancellationToken cancellationToken = default)
    {
        await eventMethodInvoker.InvokeAsync(pipelineContext, cancellationToken);

        var state = pipelineContext.Pipeline.State;

        if (state.GetHasBeenDeferred())
        {
            await projectionEventService.DeferAsync(pipelineContext, cancellationToken);
            pipelineContext.Pipeline.Abort();

            return;
        }

        await recallOptions.Value.EventProcessing.EventHandled.InvokeAsync(new(state.GetProjectionEvent(), state.GetEventEnvelope(), pipelineContext.Pipeline), cancellationToken);
    }
}