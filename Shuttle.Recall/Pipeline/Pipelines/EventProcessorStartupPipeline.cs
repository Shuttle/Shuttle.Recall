using System;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Recall;

public class EventProcessorStartupPipeline : Pipeline
{
    public EventProcessorStartupPipeline(IServiceProvider serviceProvider, IStartupEventProcessingObserver startupEventProcessingObserver) 
        : base(serviceProvider)
    {
        RegisterStage("Startup")
            .WithEvent<OnStartEventProcessing>()
            .WithEvent<OnAfterStartEventProcessing>()
            .WithEvent<OnConfigureThreadPools>()
            .WithEvent<OnAfterConfigureThreadPools>()
            .WithEvent<OnStartThreadPools>()
            .WithEvent<OnAfterStartThreadPools>();

        RegisterObserver(Guard.AgainstNull(startupEventProcessingObserver));
    }
}