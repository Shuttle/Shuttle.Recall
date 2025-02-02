using System.Threading.Tasks;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Recall;

public interface IEventStreamObserver : IPipelineObserver<OnCommitEventStream>
{
}

public class EventStreamObserver : IEventStreamObserver
{
    public async Task ExecuteAsync(IPipelineContext<OnCommitEventStream> pipelineContext)
    {
        var state = Guard.AgainstNull(pipelineContext).Pipeline.State;

        state.GetEventStream().Commit();

        await Task.CompletedTask;
    }
}