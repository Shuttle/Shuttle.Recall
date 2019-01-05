using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.PipelineTransaction;

namespace Shuttle.Recall
{
    public class EventProcessingPipeline : Pipeline
    {
        public EventProcessingPipeline(
            IProjectionEventObserver projectionEventObserver,
            IProjectionEventEnvelopeObserver projectionEventEnvelopeObserver,
            IProcessEventObserver processEventObserver, IAcknowledgeEventObserver acknowledgeEventObserver,
            ITransactionScopeObserver transactionScopeObserver)
        {
            Guard.AgainstNull(projectionEventObserver, nameof(projectionEventObserver));
            Guard.AgainstNull(projectionEventEnvelopeObserver, nameof(projectionEventEnvelopeObserver));
            Guard.AgainstNull(processEventObserver, nameof(processEventObserver));
            Guard.AgainstNull(acknowledgeEventObserver, nameof(acknowledgeEventObserver));
            Guard.AgainstNull(transactionScopeObserver, nameof(transactionScopeObserver));

            RegisterStage("Process")
                .WithEvent<OnStartTransactionScope>()
                .WithEvent<OnAfterStartTransactionScope>()
                .WithEvent<OnGetProjectionEvent>()
                .WithEvent<OnAfterGetProjectionEvent>()
                .WithEvent<OnGetProjectionEventEnvelope>()
                .WithEvent<OnAfterGetProjectionEventEnvelope>()
                .WithEvent<OnProcessEvent>()
                .WithEvent<OnAfterProcessEvent>()
                .WithEvent<OnAcknowledgeEvent>()
                .WithEvent<OnAfterAcknowledgeEvent>()
                .WithEvent<OnCompleteTransactionScope>()
                .WithEvent<OnDisposeTransactionScope>();

            RegisterObserver(projectionEventObserver);
            RegisterObserver(projectionEventEnvelopeObserver);
            RegisterObserver(processEventObserver);
            RegisterObserver(acknowledgeEventObserver);
            RegisterObserver(transactionScopeObserver);
        }
    }
}