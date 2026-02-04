using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.TransactionScope;

namespace Shuttle.Recall;

public class EventProcessorStartupPipeline : Pipeline
{
    public EventProcessorStartupPipeline(IOptions<PipelineOptions> pipelineOptions, IOptions<TransactionScopeOptions> transactionScopeOptions, ITransactionScopeFactory transactionScopeFactory, IServiceProvider serviceProvider, IStartupEventProcessingObserver startupEventProcessingObserver)
        : base(pipelineOptions, transactionScopeOptions, transactionScopeFactory, serviceProvider)
    {
        AddStage("Startup")
            .WithEvent<StartEventProcessing>()
            .WithEvent<EventProcessingStarted>()
            .WithEvent<ConfigureThreadPools>()
            .WithEvent<ThreadPoolsConfigured>()
            .WithEvent<StartThreadPools>()
            .WithEvent<ThreadPoolsStarted>();

        AddObserver(Guard.AgainstNull(startupEventProcessingObserver));
    }
}