using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Recall;

public class GetEventStreamPipeline : Pipeline
{
    public GetEventStreamPipeline(IPipelineDependencies pipelineDependencies, IGetStreamEventsObserver getStreamEventsObserver, IAssembleEventStreamObserver assembleEventStreamObserver)
        : base(pipelineDependencies)
    {
        AddStage("GetEventStream")
            .WithEvent<RetrieveStreamEvents>()
            .WithEvent<StreamEventsRetrieved>()
            .WithEvent<AssembleEventStream>()
            .WithEvent<EventStreamAssembled>();

        AddObserver(Guard.AgainstNull(getStreamEventsObserver));
        AddObserver(Guard.AgainstNull(assembleEventStreamObserver));
    }

    public async Task<EventStream> ExecuteAsync(Guid id, EventStreamBuilder builder)
    {
        State.SetId(id);
        State.SetEventStreamBuilder(builder);

        await ExecuteAsync().ConfigureAwait(false);

        return State.GetEventStream();
    }
}