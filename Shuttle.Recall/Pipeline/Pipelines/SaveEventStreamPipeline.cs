using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Recall;

public class SaveEventStreamPipeline : Pipeline
{
    public SaveEventStreamPipeline(IPipelineDependencies pipelineDependencies, IAssembleEventEnvelopesObserver assembleEventEnvelopesObserver, ISavePrimitiveEventsObserver savePrimitiveEventsObserver, IEventStreamObserver eventStreamObserver)
        : base(pipelineDependencies)
    {
        AddStage("Handle")
            .WithEvent<AssembleEventEnvelopes>()
            .WithEvent<EventEnvelopesAssembled>()
            .WithEvent<SavePrimitiveEvents>()
            .WithEvent<PrimitiveEventsSaved>()
            .WithEvent<CompleteTransactionScope>()
            .WithEvent<DisposeTransactionScope>()
            .WithEvent<CommitEventStream>()
            .WithEvent<EventStreamCommitted>();

        AddStage("Completed")
            .WithEvent<SaveEventStream>()
            .WithEvent<EventStreamSaved>()
            .WithEvent<CompleteTransactionScope>()
            .WithEvent<DisposeTransactionScope>();

        AddObserver(Guard.AgainstNull(assembleEventEnvelopesObserver));
        AddObserver(Guard.AgainstNull(savePrimitiveEventsObserver));
        AddObserver(Guard.AgainstNull(eventStreamObserver));
    }

    public async Task ExecuteAsync(EventStream eventStream, EventStreamBuilder builder)
    {
        State.SetEventStream(Guard.AgainstNull(eventStream));
        State.SetEventStreamBuilder(builder);

        await ExecuteAsync().ConfigureAwait(false);
    }
}