using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Recall;

public class EventProcessorStartupPipeline : Pipeline
{
    public EventProcessorStartupPipeline(IPipelineDependencies pipelineDependencies, IStartupEventProcessingObserver startupEventProcessingObserver)
        : base(pipelineDependencies)
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