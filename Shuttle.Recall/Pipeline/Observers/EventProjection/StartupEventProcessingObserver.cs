using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Threading;

namespace Shuttle.Recall
{
    public class StartupEventProcessingObserver : IStartupEventProcessingObserver
    {
        private readonly IProcessorThreadPoolFactory _processorThreadPoolFactory;
        private readonly IProjectionConfiguration _projectionConfiguration;
        private readonly EventStoreOptions _eventStoreOptions;
        private readonly IPipelineFactory _pipelineFactory;
        private readonly IEventProcessor _processor;
        private readonly IServiceProvider _serviceProvider;

        public StartupEventProcessingObserver(IOptions<EventStoreOptions> eventStoreOptions, IServiceProvider serviceProvider, IEventProcessor processor, IProjectionConfiguration projectionConfiguration, IPipelineFactory pipelineFactory, IProcessorThreadPoolFactory processorThreadPoolFactory)
        {
            _pipelineFactory = Guard.AgainstNull(pipelineFactory, nameof(pipelineFactory));
            _eventStoreOptions = Guard.AgainstNull(Guard.AgainstNull(eventStoreOptions, nameof(eventStoreOptions)).Value, nameof(eventStoreOptions.Value));
            _serviceProvider = Guard.AgainstNull(serviceProvider, nameof(serviceProvider));
            _processor = Guard.AgainstNull(processor, nameof(processor));
            _projectionConfiguration = Guard.AgainstNull(projectionConfiguration, nameof(projectionConfiguration));
            _processorThreadPoolFactory = Guard.AgainstNull(processorThreadPoolFactory, nameof(processorThreadPoolFactory));
        }

        public void Execute(OnStartEventProcessing pipelineEvent)
        {
            ExecuteAsync(pipelineEvent, true).GetAwaiter().GetResult();
        }

        public async Task ExecuteAsync(OnStartEventProcessing pipelineEvent)
        {
            await ExecuteAsync(pipelineEvent, false).ConfigureAwait(false);
        }

        public void Execute(OnConfigureThreadPools pipelineEvent)
        {
            var projectionCount = _projectionConfiguration.GetProjectionNames().Count();

            if (projectionCount < 1)
            {
                projectionCount = 1;
            }

            pipelineEvent.Pipeline.State.Add("EventProcessorThreadPool", _processorThreadPoolFactory.Create(
                "EventProcessorThreadPool",
                _eventStoreOptions.ProjectionThreadCount > projectionCount ? projectionCount : _eventStoreOptions.ProjectionThreadCount,
                new ProjectionProcessorFactory(_eventStoreOptions, _pipelineFactory, _processor),
                _eventStoreOptions.ProcessorThread));
        }

        public async Task ExecuteAsync(OnConfigureThreadPools pipelineEvent)
        {
            Execute(pipelineEvent);

            await Task.CompletedTask;
        }

        public void Execute(OnStartThreadPools pipelineEvent)
        {
            ExecuteAsync(pipelineEvent, true).GetAwaiter().GetResult();
        }

        public async Task ExecuteAsync(OnStartThreadPools pipelineEvent)
        {
            await ExecuteAsync(pipelineEvent, false).ConfigureAwait(false);
        }

        private async Task ExecuteAsync(OnStartEventProcessing pipelineEvent, bool sync)
        {
            Guard.AgainstNull(pipelineEvent, nameof(pipelineEvent));

            foreach (var projectionName in _projectionConfiguration.GetProjectionNames())
            {
                var projection = sync
                    ? _processor.AddProjection(projectionName)
                    : await _processor.AddProjectionAsync(projectionName);

                foreach (var type in _projectionConfiguration.GetEventHandlerTypes(projectionName))
                {
                    if (sync)
                    {
                        projection.AddEventHandler(_serviceProvider.GetService(type));
                    }
                    else
                    {
                        await projection.AddEventHandlerAsync(_serviceProvider.GetService(type));
                    }
                }
            }

            await Task.CompletedTask;
        }

        private async Task ExecuteAsync(OnStartThreadPools pipelineEvent, bool sync)
        {
            var state = Guard.AgainstNull(pipelineEvent?.Pipeline?.State, nameof(pipelineEvent.Pipeline.State));

            var eventProcessorThreadPool = state.Get<IProcessorThreadPool>("EventProcessorThreadPool");

            if (sync)
            {
                eventProcessorThreadPool.Start();
            }
            else
            {
                await eventProcessorThreadPool.StartAsync();
            }
        }
    }

    public interface IStartupEventProcessingObserver :
        IPipelineObserver<OnStartEventProcessing>,
        IPipelineObserver<OnConfigureThreadPools>,
        IPipelineObserver<OnStartThreadPools>
    {
    }
}