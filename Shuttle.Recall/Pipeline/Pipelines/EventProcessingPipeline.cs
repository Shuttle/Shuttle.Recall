using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.TransactionScope;

namespace Shuttle.Recall;

public interface IEventProcessingPipeline : IPipeline;

public class EventProcessingPipeline : Pipeline, IEventProcessingPipeline
{
    public EventProcessingPipeline(IOptions<PipelineOptions> pipelineOptions, IOptions<TransactionScopeOptions> transactionScopeOptions, ITransactionScopeFactory transactionScopeFactory, IServiceProvider serviceProvider, IProjectionEventObserver projectionEventObserver, IProjectionEventEnvelopeObserver projectionEventEnvelopeObserver, IHandleEventObserver handleEventObserver, IAcknowledgeEventObserver acknowledgeEventObserver, IEventProcessingPipelineFailedObserver eventProcessingPipelineFailedObserver)
        : base(pipelineOptions, transactionScopeOptions, transactionScopeFactory, serviceProvider)
    {
        AddStage("Handle")
            .WithEvent<RetrieveEvent>()
            .WithEvent<EventRetrieved>()
            .WithEvent<RetrieveEventEnvelope>()
            .WithEvent<EventEnvelopeRetrieved>()
            .WithEvent<HandleEvent>()
            .WithEvent<EventHandled>()
            .WithEvent<AcknowledgeEvent>()
            .WithEvent<EventAcknowledged>()
            .WithEvent<CompleteTransactionScope>()
            .WithEvent<DisposeTransactionScope>()
            .WithTransactionScope();

        AddObserver(Guard.AgainstNull(projectionEventObserver));
        AddObserver(Guard.AgainstNull(projectionEventEnvelopeObserver));
        AddObserver(Guard.AgainstNull(handleEventObserver));
        AddObserver(Guard.AgainstNull(acknowledgeEventObserver));
        AddObserver(Guard.AgainstNull(eventProcessingPipelineFailedObserver));

        AddObserver(async (IPipelineContext<PipelineFailed> context) =>
        {
            context.Pipeline.Abort();

            await Task.CompletedTask;
        });
    }
}