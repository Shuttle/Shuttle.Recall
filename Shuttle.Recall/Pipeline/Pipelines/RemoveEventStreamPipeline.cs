using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Recall;

public class RemoveEventStreamPipeline : Pipeline
{
    public RemoveEventStreamPipeline(IPipelineDependencies pipelineDependencies, IRemoveEventStreamObserver removeEventStreamObserver)
        : base(pipelineDependencies)
    {
        AddStage("RemoveEventStream")
            .WithEvent<RemoveEventStream>()
            .WithEvent<EventStreamRemoved>();

        AddObserver(Guard.AgainstNull(removeEventStreamObserver));
    }

    public async Task ExecuteAsync(Guid id)
    {
        State.SetId(id);

        await ExecuteAsync().ConfigureAwait(false);
    }
}