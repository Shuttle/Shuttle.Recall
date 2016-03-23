using Shuttle.Core.Infrastructure;

namespace Shuttle.Recall
{
    public class EventProcessingPipeline : Pipeline
    {
        public EventProcessingPipeline()
        {
	        RegisterStage("Process")
		        .WithEvent<OnStartTransactionScope>()
		        .WithEvent<OnAfterStartTransactionScope>()
		        .WithEvent<OnGetEvent>()
		        .WithEvent<OnAfterGetEvent>()
		        .WithEvent<OnProcessEvent>()
		        .WithEvent<OnAfterProcessEvent>()
		        .WithEvent<OnAcknowledgeEvent>()
		        .WithEvent<OnAfterAcknowledgeEvent>()
		        .WithEvent<OnCompleteTransactionScope>()
		        .WithEvent<OnDisposeTransactionScope>();
				;

			RegisterObserver(new GetEventObserver());
            RegisterObserver(new ProcessEventObserver());
            RegisterObserver(new AcknowledgeEventObserver());
            RegisterObserver(new TransactionScopeObserver());
        }
    }
}