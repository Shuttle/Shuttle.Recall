using System;
using System.Threading.Tasks;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Recall
{
    public class StartupEventProcessingObserver : IStartupEventProcessingObserver
    {
        private readonly IEventStoreConfiguration _eventStoreConfiguration;
        private readonly IEventProcessor _processor;
        private readonly IServiceProvider _serviceProvider;

        public StartupEventProcessingObserver(IServiceProvider serviceProvider, IEventProcessor processor, IEventStoreConfiguration eventStoreConfiguration)
        {
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
    }

    public interface IStartupEventProcessingObserver : IPipelineObserver<OnStartEventProcessingEvent>
    {
    }
}