using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Recall;

public interface IHandleEventObserver : IPipelineObserver<HandleEvent>;

public class HandleEventObserver(IEventHandlerInvoker eventMethodInvoker, IProjectionEventService projectionEventService) : IHandleEventObserver
{
    private readonly IEventHandlerInvoker _eventMethodInvoker = Guard.AgainstNull(eventMethodInvoker);
    private readonly IProjectionEventService _projectionEventService = Guard.AgainstNull(projectionEventService);

    public async Task ExecuteAsync(IPipelineContext<HandleEvent> pipelineContext, CancellationToken cancellationToken = default)
    {
        await _eventMethodInvoker.InvokeAsync(pipelineContext, cancellationToken);

        var deferredUntil = pipelineContext.Pipeline.State.GetDeferredUntil();

        if (deferredUntil.HasValue && deferredUntil > DateTimeOffset.UtcNow)
        {
            await _projectionEventService.DeferAsync(pipelineContext, cancellationToken);
            pipelineContext.Pipeline.Abort();
        }
    }
}