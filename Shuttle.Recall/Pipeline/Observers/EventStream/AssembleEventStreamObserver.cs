using Shuttle.Contract;
using Shuttle.Pipelines;

namespace Shuttle.Recall;

public interface IAssembleEventStreamObserver : IPipelineObserver<AssembleEventStream>;

public class AssembleEventStreamObserver(IEventMethodInvoker eventMethodInvoker) : IAssembleEventStreamObserver
{
    public Task ExecuteAsync(IPipelineContext<AssembleEventStream> pipelineContext, CancellationToken cancellationToken = default)
    {
        var state = Guard.AgainstNull(pipelineContext).Pipeline.State;

        state.SetEventStream(new(state.GetId(), state.GetVersion(), eventMethodInvoker, state.GetEvents()));

        return Task.CompletedTask;
    }
}