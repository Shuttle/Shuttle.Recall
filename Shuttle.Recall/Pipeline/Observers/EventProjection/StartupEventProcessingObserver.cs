using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Threading;

namespace Shuttle.Recall
{
    public class StartupEventProcessingObserver : IStartupEventProcessingObserver
    {
        private readonly IPipelineFactory _pipelineFactory;
        private readonly IEventStoreConfiguration _eventStoreConfiguration;
        private readonly IEventProcessor _processor;
        private readonly IServiceProvider _serviceProvider;
        private readonly EventStoreOptions _eventStoreOptions;

        public StartupEventProcessingObserver(IOptions<EventStoreOptions> eventStoreOptions, IServiceProvider serviceProvider, IEventProcessor processor, IEventStoreConfiguration eventStoreConfiguration, IPipelineFactory pipelineFactory)
        {
            _pipelineFactory = Guard.AgainstNull(pipelineFactory, nameof(pipelineFactory));
            _eventStoreOptions = Guard.AgainstNull(Guard.AgainstNull(eventStoreOptions, nameof(eventStoreOptions)).Value, nameof(eventStoreOptions.Value));
            _serviceProvider = Guard.AgainstNull(serviceProvider, nameof(serviceProvider));
            _processor = Guard.AgainstNull(processor, nameof(processor));
            _eventStoreConfiguration = Guard.AgainstNull(eventStoreConfiguration, nameof(eventStoreConfiguration));
        }

        public void Execute(OnStartEventProcessingEvent pipelineEvent)
        {
            ExecuteAsync(pipelineEvent).GetAwaiter().GetResult();
        }

        public async Task ExecuteAsync(OnStartEventProcessingEvent pipelineEvent)
        {
            Guard.AgainstNull(pipelineEvent, nameof(pipelineEvent));

            foreach (var projectionName in _eventStoreConfiguration.GetProjectionNames())
            {
                var projection = _processor.AddProjection(projectionName);

                foreach (var type in _eventStoreConfiguration.GetEventHandlerTypes(projectionName))
                {
                    projection.AddEventHandler(_serviceProvider.GetService(type));
                }
            }

            await Task.CompletedTask;
        }

        public void Execute(OnConfigureThreadPools pipelineEvent)
        {
            pipelineEvent.Pipeline.State.Add("EventProcessorThreadPool", new ProcessorThreadPool(
                "EventProcessorThreadPool ",
                _eventStoreOptions.ProjectionThreadCount,
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
        IPipelineObserver<OnStartEventProcessingEvent>,
        IPipelineObserver<OnConfigureThreadPools>,
        IPipelineObserver<OnStartThreadPools>
    {
    }
}