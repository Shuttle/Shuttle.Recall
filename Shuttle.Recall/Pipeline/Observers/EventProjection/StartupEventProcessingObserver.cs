using System;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Recall
{
    public class StartupEventProcessingObserver : IStartupEventProcessingObserver
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IEventProcessor _processor;
        private readonly IEventStoreConfiguration _eventStoreConfiguration;

        public StartupEventProcessingObserver(IServiceProvider serviceProvider, IEventProcessor processor, IEventStoreConfiguration eventStoreConfiguration)
        {
            Guard.AgainstNull(serviceProvider, nameof(serviceProvider));
            Guard.AgainstNull(processor, nameof(processor));
            Guard.AgainstNull(eventStoreConfiguration, nameof(eventStoreConfiguration));

            _serviceProvider = serviceProvider;
            _processor = processor;
            _eventStoreConfiguration = eventStoreConfiguration;
        }

        public void Execute(OnStartEventProcessingEvent pipelineEvent)
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
        }
    }

    public interface IStartupEventProcessingObserver :
        IPipelineObserver<OnStartEventProcessingEvent>
    {
    }
}