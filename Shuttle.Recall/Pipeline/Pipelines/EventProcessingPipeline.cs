using Shuttle.Core.Infrastructure;

namespace Shuttle.Recall
{
    public class EventProcessingPipeline : Pipeline
    {
        public EventProcessingPipeline(GetProjectionPrimitiveEventObserver getProjectionPrimitiveEventObserver, ProcessEventObserver processEventObserver, AcknowledgeEventObserver acknowledgeEventObserver, TransactionScopeObserver transactionScopeObserver)
        {
            Guard.AgainstNull(getProjectionPrimitiveEventObserver, "getProjectionPrimitiveEventObserver");
            Guard.AgainstNull(processEventObserver, "processEventObserver");
            Guard.AgainstNull(acknowledgeEventObserver, "acknowledgeEventObserver");
            Guard.AgainstNull(transactionScopeObserver, "TransactionScopeObserver");

	        RegisterStage("Process")
		        .WithEvent<OnStartTransactionScope>()
		        .WithEvent<OnAfterStartTransactionScope>()
		        .WithEvent<OnGetProjectionPrimitiveEvent>()
		        .WithEvent<OnAfterGetProjectionPrimitiveEvent>()
		        .WithEvent<OnProcessEvent>()
		        .WithEvent<OnAfterProcessEvent>()
		        .WithEvent<OnAcknowledgeEvent>()
		        .WithEvent<OnAfterAcknowledgeEvent>()
		        .WithEvent<OnCompleteTransactionScope>()
		        .WithEvent<OnDisposeTransactionScope>();

			RegisterObserver(getProjectionPrimitiveEventObserver);
            RegisterObserver(processEventObserver);
            RegisterObserver(acknowledgeEventObserver);
            RegisterObserver(transactionScopeObserver);
        }
    }
}