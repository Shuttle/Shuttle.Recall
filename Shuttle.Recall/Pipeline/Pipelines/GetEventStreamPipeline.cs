using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.TransactionScope;

namespace Shuttle.Recall;

public class GetEventStreamPipeline : Pipeline
{
    public GetEventStreamPipeline(IOptions<PipelineOptions> pipelineOptions, IOptions<TransactionScopeOptions> transactionScopeOptions, ITransactionScopeFactory transactionScopeFactory, IServiceProvider serviceProvider, IRetrieveStreamEventsObserver retrieveStreamEventsObserver, IAssembleEventStreamObserver assembleEventStreamObserver)
        : base(pipelineOptions, transactionScopeOptions, transactionScopeFactory, serviceProvider)
    {
        AddStage("GetEventStream")
            .WithEvent<RetrieveStreamEvents>()
            .WithEvent<StreamEventsRetrieved>()
            .WithEvent<AssembleEventStream>()
            .WithEvent<EventStreamAssembled>();

        AddObserver(Guard.AgainstNull(retrieveStreamEventsObserver));
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