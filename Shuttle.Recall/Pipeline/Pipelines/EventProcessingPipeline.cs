using Shuttle.Core.Infrastructure;

namespace Shuttle.Recall
{
    public class EventProcessingPipeline : Pipeline
    {
        public EventProcessingPipeline(ProjectionPrimitiveEventObserver projectionPrimitiveEventObserver, ProjectionEventEnvelopeObserver projectionEventEnvelopeObserver, ProcessEventObserver processEventObserver, AcknowledgeEventObserver acknowledgeEventObserver, TransactionScopeObserver transactionScopeObserver)
        {
            Guard.AgainstNull(projectionPrimitiveEventObserver, "projectionPrimitiveEventObserver");
            Guard.AgainstNull(projectionEventEnvelopeObserver, "projectionEventEnvelopeObserver");
            Guard.AgainstNull(processEventObserver, "processEventObserver");
            Guard.AgainstNull(acknowledgeEventObserver, "acknowledgeEventObserver");
            Guard.AgainstNull(transactionScopeObserver, "TransactionScopeObserver");

            RegisterStage("Process")
                .WithEvent<OnCreateEventStoreConnection>()
                .WithEvent<OnGetProjectionSequenceNumber>()
                .WithEvent<OnGetProjectionPrimitiveEvent>()
                .WithEvent<OnAfterGetProjectionPrimitiveEvent>()
                .WithEvent<OnDisposeEventStoreConnection>()
                .WithEvent<OnGetProjectionEventEnvelope>()
                .WithEvent<OnAfterGetProjectionEventEnvelope>()
                .WithEvent<OnStartTransactionScope>()
                .WithEvent<OnAfterStartTransactionScope>()
                .WithEvent<OnCreateProjectionConnection>()
                .WithEvent<OnProcessEvent>()
                .WithEvent<OnAfterProcessEvent>()
                .WithEvent<OnAcknowledgeEvent>()
                .WithEvent<OnAfterAcknowledgeEvent>()
                .WithEvent<OnCompleteTransactionScope>()
                .WithEvent<OnDisposeTransactionScope>()
                .WithEvent<OnDisposeProjectionConnection>();

            RegisterObserver(projectionPrimitiveEventObserver);
            RegisterObserver(processEventObserver);
            RegisterObserver(acknowledgeEventObserver);
            RegisterObserver(transactionScopeObserver);
        }
    }
}