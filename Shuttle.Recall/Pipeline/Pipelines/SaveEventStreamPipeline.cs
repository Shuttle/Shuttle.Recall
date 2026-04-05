using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Recall;

public interface ISaveEventStreamPipeline : IPipeline
{
    Task ExecuteAsync(EventStream eventStream, EventStreamBuilder builder);
}

public class SaveEventStreamPipeline : Pipeline, ISaveEventStreamPipeline
{
    public SaveEventStreamPipeline(IOptions<PipelineOptions> pipelineOptions, IServiceProvider serviceProvider, IAssembleEventEnvelopesObserver assembleEventEnvelopesObserver, ISavePrimitiveEventsObserver savePrimitiveEventsObserver, IEventStreamObserver eventStreamObserver)
        : base(pipelineOptions, serviceProvider)
    {
        AddStage("Assemble")
            .WithEvent<AssembleEventEnvelopes>()
            .WithEvent<EventEnvelopesAssembled>()
            .WithEvent<SavePrimitiveEvents>()
            .WithEvent<PrimitiveEventsSaved>()
            .WithEvent<CommitEventStream>()
            .WithEvent<EventStreamCommitted>();

        AddStage("Persist")
            .WithEvent<SaveEventStream>()
            .WithEvent<EventStreamSaved>();

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