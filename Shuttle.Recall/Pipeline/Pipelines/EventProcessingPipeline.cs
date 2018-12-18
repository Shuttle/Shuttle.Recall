using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.PipelineTransaction;

namespace Shuttle.Recall
{
    public class EventProcessingPipeline : Pipeline
    {
        public EventProcessingPipeline(
            IProjectionPrimitiveEventObserver projectionPrimitiveEventObserver,
            IProjectionEventEnvelopeObserver projectionEventEnvelopeObserver,
            IProcessEventObserver processEventObserver, IAcknowledgeEventObserver acknowledgeEventObserver,
            ITransactionScopeObserver transactionScopeObserver)
        {
            Guard.AgainstNull(projectionPrimitiveEventObserver, nameof(projectionPrimitiveEventObserver));
            Guard.AgainstNull(projectionEventEnvelopeObserver, nameof(projectionEventEnvelopeObserver));
            Guard.AgainstNull(processEventObserver, nameof(processEventObserver));
            Guard.AgainstNull(acknowledgeEventObserver, nameof(acknowledgeEventObserver));
            Guard.AgainstNull(transactionScopeObserver, nameof(transactionScopeObserver));

            RegisterStage("Process")
                .WithEvent<OnStartTransactionScope>()
                .WithEvent<OnAfterStartTransactionScope>()
                .WithEvent<OnGetProjectionPrimitiveEvent>()
                .WithEvent<OnAfterGetProjectionPrimitiveEvent>()
                .WithEvent<OnGetProjectionEventEnvelope>()
                .WithEvent<OnAfterGetProjectionEventEnvelope>()
                .WithEvent<OnProcessEvent>()
                .WithEvent<OnAfterProcessEvent>()
                .WithEvent<OnAcknowledgeEvent>()
                .WithEvent<OnAfterAcknowledgeEvent>()
                .WithEvent<OnCompleteTransactionScope>()
                .WithEvent<OnDisposeTransactionScope>();

            RegisterObserver(projectionPrimitiveEventObserver);
            RegisterObserver(projectionEventEnvelopeObserver);
            RegisterObserver(processEventObserver);
            RegisterObserver(acknowledgeEventObserver);
            RegisterObserver(transactionScopeObserver);
        }
    }
}