using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shuttle.Contract;
using Shuttle.Pipelines;
using Shuttle.Threading;

namespace Shuttle.Recall;

public class StartupEventProcessingObserver(IOptions<RecallOptions> recallOptions, IOptions<ThreadingOptions> threadingOptions, IServiceScopeFactory serviceScopeFactory, IProcessorIdleStrategy processorIdleStrategy, IEventProcessorConfiguration eventProcessorConfiguration)
    : IStartupEventProcessingObserver
{
    private readonly IProcessorIdleStrategy _processorIdleStrategy = Guard.AgainstNull(processorIdleStrategy);
    private readonly IServiceScopeFactory _serviceScopeFactory = Guard.AgainstNull(serviceScopeFactory);
    private readonly ThreadingOptions _threadingOptions = Guard.AgainstNull(Guard.AgainstNull(threadingOptions).Value);
    private readonly RecallOptions _recallOptions = Guard.AgainstNull(Guard.AgainstNull(recallOptions).Value);
    private readonly IEventProcessorConfiguration _eventProcessorConfiguration = Guard.AgainstNull(eventProcessorConfiguration);

    public async Task ExecuteAsync(IPipelineContext<ConfigureThreadPools> pipelineContext, CancellationToken cancellationToken = default)
    {
        if (_eventProcessorConfiguration.HasProjections)
        {
            var threadCount = _recallOptions.EventProcessing.ProjectionThreadCount;
            var projectionCount = _eventProcessorConfiguration.Projections.Count();

            if (threadCount > projectionCount)
            {
                threadCount = projectionCount;
            }

            Guard.AgainstNull(pipelineContext).Pipeline.State
                .Add("ProjectionProcessorThreadPool", new ProcessorThreadPool("ProjectionProcessor", threadCount, _serviceScopeFactory, _threadingOptions, _processorIdleStrategy));
        }
        
        await Task.CompletedTask;
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