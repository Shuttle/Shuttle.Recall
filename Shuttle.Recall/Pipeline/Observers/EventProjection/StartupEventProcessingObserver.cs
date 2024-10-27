using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
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
    private readonly IProjectionConfiguration _projectionConfiguration;
    private readonly IServiceProvider _serviceProvider;

    public StartupEventProcessingObserver(IOptions<EventStoreOptions> eventStoreOptions, IServiceProvider serviceProvider, IEventProcessor processor, IProjectionConfiguration projectionConfiguration, IPipelineFactory pipelineFactory, IProcessorThreadPoolFactory processorThreadPoolFactory)
    {
        _eventStoreOptions = Guard.AgainstNull(Guard.AgainstNull(eventStoreOptions).Value);
        _pipelineFactory = Guard.AgainstNull(pipelineFactory);
        _serviceProvider = Guard.AgainstNull(serviceProvider);
        _processor = Guard.AgainstNull(processor);
        _projectionConfiguration = Guard.AgainstNull(projectionConfiguration);
        _processorThreadPoolFactory = Guard.AgainstNull(processorThreadPoolFactory);
    }

    public async Task ExecuteAsync(IPipelineContext<OnStartEventProcessing> pipelineContext)
    {
        foreach (var projectionName in _projectionConfiguration.GetProjectionNames())
        {
            var projection = await _processor.AddProjectionAsync(projectionName);

            if (projection == null)
            {
                continue;
            }

            foreach (var type in _projectionConfiguration.GetEventHandlerTypes(projectionName))
            {
                await projection.AddEventHandlerAsync(_serviceProvider.GetRequiredService(type));
            }
        }

        await Task.CompletedTask;
    }

    public async Task ExecuteAsync(IPipelineContext<OnConfigureThreadPools> pipelineContext)
    {
        var projectionCount = _projectionConfiguration.GetProjectionNames().Count();

        if (projectionCount < 1)
        {
            projectionCount = 1;
        }

        Guard.AgainstNull(pipelineContext).Pipeline.State.Add(
            "EventProcessorThreadPool",
            _processorThreadPoolFactory.Create(
                "EventProcessorThreadPool",
                _eventStoreOptions.ProjectionThreadCount > projectionCount ? projectionCount : _eventStoreOptions.ProjectionThreadCount,
                new ProjectionProcessorFactory(_eventStoreOptions, _pipelineFactory, _processor),
                _eventStoreOptions.ProcessorThread)
            );

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
    IPipelineObserver<OnStartEventProcessing>,
    IPipelineObserver<OnConfigureThreadPools>,
    IPipelineObserver<OnStartThreadPools>
{
}