using System;
using System.Threading.Tasks;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Recall;

public class SaveEventStreamPipeline : Pipeline
{
    public SaveEventStreamPipeline(IServiceProvider serviceProvider, IAssembleEventEnvelopesObserver assembleEventEnvelopesObserver, ISavePrimitiveEventsObserver savePrimitiveEventsObserver, IEventStreamObserver eventStreamObserver) 
        : base(serviceProvider)
    {
        AddStage("Handle")
            .WithEvent<OnAssembleEventEnvelopes>()
            .WithEvent<OnAfterAssembleEventEnvelopes>()
            .WithEvent<OnBeforeSavePrimitiveEvents>()
            .WithEvent<OnSavePrimitiveEvents>()
            .WithEvent<OnAfterSavePrimitiveEvents>()
            .WithEvent<OnCommitEventStream>()
            .WithEvent<OnAfterCommitEventStream>();

        AddStage("Completed")
            .WithEvent<OnSaveEventStreamCompleted>()
            .WithEvent<OnAfterSaveEventStreamCompleted>();

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