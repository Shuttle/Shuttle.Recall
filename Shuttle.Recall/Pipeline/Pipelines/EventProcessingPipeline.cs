using Shuttle.Core.Infrastructure;

namespace Shuttle.Recall
{
    public class EventProcessingPipeline : Pipeline
    {
        public EventProcessingPipeline(GetProjectionSequenceNumberObserver getProjectionSequenceNumberObserver, ProjectionPrimitiveEventObserver projectionPrimitiveEventObserver, ProjectionEventEnvelopeObserver projectionEventEnvelopeObserver, ProcessEventObserver processEventObserver, AcknowledgeEventObserver acknowledgeEventObserver, TransactionScopeObserver transactionScopeObserver)
        {
            Guard.AgainstNull(getProjectionSequenceNumberObserver, "getProjectionSequenceNumberObserver");
            Guard.AgainstNull(projectionPrimitiveEventObserver, "projectionPrimitiveEventObserver");
            Guard.AgainstNull(projectionEventEnvelopeObserver, "projectionEventEnvelopeObserver");
            Guard.AgainstNull(processEventObserver, "processEventObserver");
            Guard.AgainstNull(acknowledgeEventObserver, "acknowledgeEventObserver");
            Guard.AgainstNull(transactionScopeObserver, "TransactionScopeObserver");

            RegisterStage("Process")
                .WithEvent<OnStartTransactionScope>()
                .WithEvent<OnAfterStartTransactionScope>()
                .WithEvent<OnGetProjectionSequenceNumber>()
                .WithEvent<OnAfterGetProjectionSequenceNumber>()
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

            RegisterObserver(getProjectionSequenceNumberObserver);
            RegisterObserver(projectionPrimitiveEventObserver);
            RegisterObserver(projectionEventEnvelopeObserver);
            RegisterObserver(processEventObserver);
            RegisterObserver(acknowledgeEventObserver);
            RegisterObserver(transactionScopeObserver);
        }
    }
}