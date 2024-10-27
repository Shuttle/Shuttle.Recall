using System.Threading.Tasks;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Recall;

public class SaveEventStreamPipeline : Pipeline
{
    public SaveEventStreamPipeline(IAssembleEventEnvelopesObserver assembleEventEnvelopesObserver, ISavePrimitiveEventsObserver savePrimitiveEventsObserver, IEventStreamObserver eventStreamObserver)
    {
        RegisterStage("SaveEventStream")
            .WithEvent<OnAssembleEventEnvelopes>()
            .WithEvent<OnAfterAssembleEventEnvelopes>()
            .WithEvent<OnBeforeSavePrimitiveEvents>()
            .WithEvent<OnSavePrimitiveEvents>()
            .WithEvent<OnAfterSavePrimitiveEvents>()
            .WithEvent<OnCommitEventStream>()
            .WithEvent<OnAfterCommitEventStream>();

        RegisterObserver(Guard.AgainstNull(assembleEventEnvelopesObserver));
        RegisterObserver(Guard.AgainstNull(savePrimitiveEventsObserver));
        RegisterObserver(Guard.AgainstNull(eventStreamObserver));
    }

    public async Task ExecuteAsync(EventStream eventStream, EventStreamBuilder builder)
    {
        State.SetEventStream(Guard.AgainstNull(eventStream));
        State.SetEventStreamBuilder(builder);

        await ExecuteAsync().ConfigureAwait(false);
    }
}