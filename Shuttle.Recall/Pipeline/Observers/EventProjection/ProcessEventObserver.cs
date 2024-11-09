using System.Threading.Tasks;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Recall;

public interface IHandleEventObserver : IPipelineObserver<OnHandleEvent>
{
}

public class HandleEventObserver : IHandleEventObserver
{
    private readonly IEventHandlerInvoker _eventMethodInvoker;

    public HandleEventObserver(IEventHandlerInvoker eventMethodInvoker)
    {
        _eventMethodInvoker = Guard.AgainstNull(eventMethodInvoker);
    }

    public async Task ExecuteAsync(IPipelineContext<OnHandleEvent> pipelineContext)
    {
        await _eventMethodInvoker.InvokeAsync(pipelineContext);
    }
}