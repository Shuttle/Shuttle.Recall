using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Recall;

public interface IEventStreamObserver : IPipelineObserver<CommitEventStream>;

public class EventStreamObserver : IEventStreamObserver
{
    public async Task ExecuteAsync(IPipelineContext<CommitEventStream> pipelineContext, CancellationToken cancellationToken = default)
    {
        var state = Guard.AgainstNull(pipelineContext).Pipeline.State;

        state.GetEventStream().Commit();

        await Task.CompletedTask;
    }
}