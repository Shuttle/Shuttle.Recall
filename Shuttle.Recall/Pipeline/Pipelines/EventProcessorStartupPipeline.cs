using Microsoft.Extensions.Options;
using Shuttle.Contract;
using Shuttle.Pipelines;

namespace Shuttle.Recall;

public interface IEventProcessorStartupPipeline : IPipeline;

public class EventProcessorStartupPipeline : Pipeline, IEventProcessorStartupPipeline
{
    public EventProcessorStartupPipeline(IOptions<PipelineOptions> pipelineOptions, IServiceProvider serviceProvider, IStartupEventProcessingObserver startupEventProcessingObserver)
        : base(pipelineOptions, serviceProvider)
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