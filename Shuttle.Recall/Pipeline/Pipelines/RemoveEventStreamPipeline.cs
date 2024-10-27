using System;
using System.Threading.Tasks;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Recall;

public class RemoveEventStreamPipeline : Pipeline
{
    public RemoveEventStreamPipeline(IRemoveEventStreamObserver removeEventStreamObserver)
    {
        RegisterStage("RemoveEventStream")
            .WithEvent<OnBeforeRemoveEventStream>()
            .WithEvent<OnRemoveEventStream>()
            .WithEvent<OnAfterRemoveEventStream>();

        RegisterObserver(Guard.AgainstNull(removeEventStreamObserver));
    }

    public async Task<EventStream> ExecuteAsync(Guid id, EventStreamBuilder builder)
    {
        State.SetId(id);
        State.SetEventStreamBuilder(builder);

        await ExecuteAsync().ConfigureAwait(false);

        return State.GetEventStream();
    }
}