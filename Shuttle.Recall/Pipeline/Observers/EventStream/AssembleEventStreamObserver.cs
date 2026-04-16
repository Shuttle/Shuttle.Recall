using Shuttle.Contract;
using Shuttle.Pipelines;

namespace Shuttle.Recall;

public interface IAssembleEventStreamObserver : IPipelineObserver<AssembleEventStream>;

public class AssembleEventStreamObserver(IEventMethodInvoker eventMethodInvoker) : IAssembleEventStreamObserver
{
    private readonly IEventMethodInvoker _eventMethodInvoker = Guard.AgainstNull(eventMethodInvoker);

    public async Task ExecuteAsync(IPipelineContext<AssembleEventStream> pipelineContext, CancellationToken cancellationToken = default)
    {
        var state = Guard.AgainstNull(pipelineContext).Pipeline.State;

        state.SetEventStream(new(state.GetId(), state.GetVersion(), _eventMethodInvoker, state.GetEvents()));

        await Task.CompletedTask;
    }
}