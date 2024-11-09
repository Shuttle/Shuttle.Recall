using System.Threading.Tasks;
using Shuttle.Core.Pipelines;

namespace Shuttle.Recall;

public interface IEventHandlerInvoker
{
    ValueTask<bool> InvokeAsync(IPipelineContext<OnHandleEvent> pipelineContext);
}