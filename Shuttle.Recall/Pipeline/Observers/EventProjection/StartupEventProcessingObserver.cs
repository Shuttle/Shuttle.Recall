using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Recall
{
    public class StartupEventProcessingObserver : IStartupEventProcessingObserver
    {
        private readonly IEventProcessor _processor;
        private readonly IEventStoreConfiguration _eventStoreConfiguration;

        public StartupEventProcessingObserver(IEventProcessor processor, IEventStoreConfiguration eventStoreConfiguration)
        {
            Guard.AgainstNull(processor, nameof(processor));
            Guard.AgainstNull(eventStoreConfiguration, nameof(eventStoreConfiguration));

            _processor = processor;
            _eventStoreConfiguration = eventStoreConfiguration;
        }

        public void Execute(OnStartEventProcessingEvent pipelineEvent)
        {
            Guard.AgainstNull(pipelineEvent, nameof(pipelineEvent));

            foreach (var projectionName in _eventStoreConfiguration.GetProjectionNames())
            {
                _processor.AddProjection(projectionName);
            }
        }
    }

    public interface IStartupEventProcessingObserver :
        IPipelineObserver<OnStartEventProcessingEvent>
    {
    }
}