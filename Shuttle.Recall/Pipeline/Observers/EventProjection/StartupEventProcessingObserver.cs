using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Threading;

namespace Shuttle.Recall;

public class StartupEventProcessingObserver : IStartupEventProcessingObserver
{
    private readonly EventStoreOptions _eventStoreOptions;
    private readonly IPipelineFactory _pipelineFactory;
    private readonly IEventProcessor _processor;
    private readonly IProcessorThreadPoolFactory _processorThreadPoolFactory;

    public StartupEventProcessingObserver(IOptions<EventStoreOptions> eventStoreOptions, IEventProcessor processor, IPipelineFactory pipelineFactory, IProcessorThreadPoolFactory processorThreadPoolFactory)
    {
        _eventStoreOptions = Guard.AgainstNull(Guard.AgainstNull(eventStoreOptions).Value);
        _pipelineFactory = Guard.AgainstNull(pipelineFactory);
        _processor = Guard.AgainstNull(processor);
        _processorThreadPoolFactory = Guard.AgainstNull(processorThreadPoolFactory);
    }

    public async Task ExecuteAsync(IPipelineContext<OnConfigureThreadPools> pipelineContext)
    {
        Guard.AgainstNull(pipelineContext).Pipeline.State
            .Add("EventProcessorThreadPool", _processorThreadPoolFactory.Create("EventProcessorThreadPool", _eventStoreOptions.ProjectionThreadCount, new ProjectionProcessorFactory(_eventStoreOptions, _pipelineFactory, _processor), _eventStoreOptions.ProcessorThread));

        await Task.CompletedTask;
    }

    public async Task ExecuteAsync(IPipelineContext<OnStartThreadPools> pipelineContext)
    {
        var state = Guard.AgainstNull(pipelineContext).Pipeline.State;

        var eventProcessorThreadPool = Guard.AgainstNull(state.Get<IProcessorThreadPool>("EventProcessorThreadPool"));

        await eventProcessorThreadPool.StartAsync();
    }
}

public interface IStartupEventProcessingObserver :
    IPipelineObserver<OnConfigureThreadPools>,
    IPipelineObserver<OnStartThreadPools>
{
}