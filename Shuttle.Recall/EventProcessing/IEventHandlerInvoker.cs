using Shuttle.Pipelines;

namespace Shuttle.Recall;

public interface IEventHandlerInvoker
{
    ValueTask<bool> InvokeAsync(IPipelineContext<HandleEvent> pipelineContext, CancellationToken cancellationToken = default);
}