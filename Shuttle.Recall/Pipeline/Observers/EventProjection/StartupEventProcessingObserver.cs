using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Threading;

namespace Shuttle.Recall;

public class StartupEventProcessingObserver(IOptions<EventStoreOptions> eventStoreOptions, IPipelineFactory pipelineFactory, IProcessorThreadPoolFactory processorThreadPoolFactory)
    : IStartupEventProcessingObserver
{
    private readonly EventStoreOptions _eventStoreOptions = Guard.AgainstNull(Guard.AgainstNull(eventStoreOptions).Value);
    private readonly IPipelineFactory _pipelineFactory = Guard.AgainstNull(pipelineFactory);
    private readonly IProcessorThreadPoolFactory _processorThreadPoolFactory = Guard.AgainstNull(processorThreadPoolFactory);

    public async Task ExecuteAsync(IPipelineContext<ConfigureThreadPools> pipelineContext, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(pipelineContext).Pipeline.State
            .Add("EventProcessorThreadPool", await _processorThreadPoolFactory.CreateAsync("EventProcessorThreadPool", _eventStoreOptions.ProjectionThreadCount, new ProjectionProcessorFactory(_eventStoreOptions, _pipelineFactory), cancellationToken));

        await Task.CompletedTask;
    }

    public async Task ExecuteAsync(IPipelineContext<StartThreadPools> pipelineContext, CancellationToken cancellationToken = default)
    {
        var state = Guard.AgainstNull(pipelineContext).Pipeline.State;

        var eventProcessorThreadPool = Guard.AgainstNull(state.Get<IProcessorThreadPool>("EventProcessorThreadPool"));

        await eventProcessorThreadPool.StartAsync(cancellationToken);
    }
}

public interface IStartupEventProcessingObserver :
    IPipelineObserver<ConfigureThreadPools>,
    IPipelineObserver<StartThreadPools>;