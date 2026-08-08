using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shuttle.Contract;
using Shuttle.Pipelines;
using Shuttle.Threading;

namespace Shuttle.Recall;

public class StartupEventProcessingObserver(IOptions<RecallOptions> recallOptions, IOptions<ThreadingOptions> threadingOptions, IServiceScopeFactory serviceScopeFactory, IProcessorIdleStrategy processorIdleStrategy, IEventProcessorConfiguration eventProcessorConfiguration)
    : IStartupEventProcessingObserver
{
    public Task ExecuteAsync(IPipelineContext<ConfigureThreadPools> pipelineContext, CancellationToken cancellationToken = default)
    {
        if (eventProcessorConfiguration.HasProjections)
        {
            var threadCount = recallOptions.Value.EventProcessing.ProjectionThreadCount;
            var projectionCount = eventProcessorConfiguration.Projections.Count();

            if (threadCount > projectionCount)
            {
                threadCount = projectionCount;
            }

            Guard.AgainstNull(pipelineContext).Pipeline.State
                .Add("ProjectionProcessorThreadPool", new ProcessorThreadPool("ProjectionProcessor", threadCount, serviceScopeFactory, threadingOptions.Value, processorIdleStrategy));
        }
        
        return Task.CompletedTask;
    }

    public async Task ExecuteAsync(IPipelineContext<StartThreadPools> pipelineContext, CancellationToken cancellationToken = default)
    {
        var state = Guard.AgainstNull(pipelineContext).Pipeline.State;
        var threadPool = state.Get<IProcessorThreadPool>("ProjectionProcessorThreadPool");

        if (threadPool != null)
        {
            await threadPool.StartAsync(cancellationToken);
        }
    }
}

public interface IStartupEventProcessingObserver :
    IPipelineObserver<ConfigureThreadPools>,
    IPipelineObserver<StartThreadPools>;