using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.PipelineTransaction;

namespace Shuttle.Recall
{
    public class EventProcessorStartupPipeline : Pipeline
    {
        public EventProcessorStartupPipeline(IStartupEventProcessingObserver startupEventProcessingObserver)
        {
            Guard.AgainstNull(startupEventProcessingObserver, nameof(startupEventProcessingObserver));

            RegisterStage("Process")
                .WithEvent<OnStartTransactionScope>()
                .WithEvent<OnAfterStartTransactionScope>()
                .WithEvent<OnBeforeStartEventProcessingEvent>()
                .WithEvent<OnStartEventProcessingEvent>()
                .WithEvent<OnAfterStartEventProcessingEvent>()
                .WithEvent<OnCompleteTransactionScope>()
                .WithEvent<OnDisposeTransactionScope>()
                .WithEvent<OnConfigureThreadPools>()
                .WithEvent<OnAfterConfigureThreadPools>()
                .WithEvent<OnStartThreadPools>()
                .WithEvent<OnAfterStartThreadPools>();

            RegisterObserver(startupEventProcessingObserver);
        }
    }
}