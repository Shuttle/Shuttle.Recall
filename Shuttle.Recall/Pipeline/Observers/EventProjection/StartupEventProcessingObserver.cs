using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Threading;

namespace Shuttle.Recall;

public class StartupEventProcessingObserver(IOptions<EventStoreOptions> eventStoreOptions, IOptions<ThreadingOptions> threadingOptions, IServiceScopeFactory serviceScopeFactory, IProcessorIdleStrategy processorIdleStrategy)
    : IStartupEventProcessingObserver
{
    private readonly IProcessorIdleStrategy _processorIdleStrategy = Guard.AgainstNull(processorIdleStrategy);
    private readonly IServiceScopeFactory _serviceScopeFactory = Guard.AgainstNull(serviceScopeFactory);
    private readonly ThreadingOptions _threadingOptions = Guard.AgainstNull(Guard.AgainstNull(threadingOptions).Value);
    private readonly EventStoreOptions _eventStoreOptions = Guard.AgainstNull(Guard.AgainstNull(eventStoreOptions).Value);

    public async Task ExecuteAsync(IPipelineContext<ConfigureThreadPools> pipelineContext, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(pipelineContext).Pipeline.State
            .Add("ProjectionProcessorThreadPool", new ProcessorThreadPool("ProjectionProcessor", _eventStoreOptions.ProjectionThreadCount, _serviceScopeFactory, _threadingOptions, _processorIdleStrategy));

        await Task.CompletedTask;
    }

    public async Task ExecuteAsync(IPipelineContext<StartThreadPools> pipelineContext, CancellationToken cancellationToken = default)
    {
        var state = Guard.AgainstNull(pipelineContext).Pipeline.State;

        await Guard.AgainstNull(state.Get<IProcessorThreadPool>("ProjectionProcessorThreadPool")).StartAsync(cancellationToken);
    }
}

public interface IStartupEventProcessingObserver :
    IPipelineObserver<ConfigureThreadPools>,
    IPipelineObserver<StartThreadPools>;