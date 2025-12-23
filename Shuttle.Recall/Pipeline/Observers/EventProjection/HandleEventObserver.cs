using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Recall;

public interface IHandleEventObserver : IPipelineObserver<HandleEvent>;

public class HandleEventObserver(IEventHandlerInvoker eventMethodInvoker) : IHandleEventObserver
{
    private readonly IEventHandlerInvoker _eventMethodInvoker = Guard.AgainstNull(eventMethodInvoker);

    public async Task ExecuteAsync(IPipelineContext<HandleEvent> pipelineContext, CancellationToken cancellationToken = default)
    {
        await _eventMethodInvoker.InvokeAsync(pipelineContext, cancellationToken);
    }
}