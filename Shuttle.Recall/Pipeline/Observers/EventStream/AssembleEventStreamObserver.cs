using System.Threading.Tasks;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Recall;

public interface IAssembleEventStreamObserver : IPipelineObserver<OnAssembleEventStream>
{
}

public class AssembleEventStreamObserver : IAssembleEventStreamObserver
{
    private readonly IEventMethodInvoker _eventMethodInvoker;

    public AssembleEventStreamObserver(IEventMethodInvoker eventMethodInvoker)
    {
        _eventMethodInvoker = Guard.AgainstNull(eventMethodInvoker);
    }

    public async Task ExecuteAsync(IPipelineContext<OnAssembleEventStream> pipelineContext)
    {
        var state = Guard.AgainstNull(pipelineContext).Pipeline.State;

        state.SetEventStream(new(state.GetId(), state.GetVersion(), _eventMethodInvoker, state.GetEvents()));

        await Task.CompletedTask;
    }
}