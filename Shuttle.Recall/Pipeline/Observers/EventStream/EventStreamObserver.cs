using Shuttle.Contract;
using Shuttle.Pipelines;

namespace Shuttle.Recall;

public interface IEventStreamObserver : IPipelineObserver<CommitEventStream>;

public class EventStreamObserver : IEventStreamObserver
{
    public Task ExecuteAsync(IPipelineContext<CommitEventStream> pipelineContext, CancellationToken cancellationToken = default)
    {
        var state = Guard.AgainstNull(pipelineContext).Pipeline.State;

        state.GetEventStream().Commit();

        return Task.CompletedTask;
    }
}