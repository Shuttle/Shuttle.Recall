using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.TransactionScope;

namespace Shuttle.Recall;

public interface ISaveEventStreamPipeline : IPipeline
{
    Task ExecuteAsync(EventStream eventStream, EventStreamBuilder builder);
}

public class SaveEventStreamPipeline : Pipeline, ISaveEventStreamPipeline
{
    public SaveEventStreamPipeline(IOptions<PipelineOptions> pipelineOptions, IOptions<TransactionScopeOptions> transactionScopeOptions, ITransactionScopeFactory transactionScopeFactory, IServiceProvider serviceProvider, IAssembleEventEnvelopesObserver assembleEventEnvelopesObserver, ISavePrimitiveEventsObserver savePrimitiveEventsObserver, IEventStreamObserver eventStreamObserver)
        : base(pipelineOptions, transactionScopeOptions, transactionScopeFactory, serviceProvider)
    {
        AddStage("Handle")
            .WithEvent<AssembleEventEnvelopes>()
            .WithEvent<EventEnvelopesAssembled>()
            .WithEvent<SavePrimitiveEvents>()
            .WithEvent<PrimitiveEventsSaved>()
            .WithEvent<CompleteTransactionScope>()
            .WithEvent<DisposeTransactionScope>()
            .WithEvent<CommitEventStream>()
            .WithEvent<EventStreamCommitted>()
            .WithTransactionScope();

        AddStage("Completed")
            .WithEvent<SaveEventStream>()
            .WithEvent<EventStreamSaved>()
            .WithEvent<CompleteTransactionScope>()
            .WithEvent<DisposeTransactionScope>()
            .WithTransactionScope();

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