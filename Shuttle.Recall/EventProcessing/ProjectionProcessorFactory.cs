using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Threading;

namespace Shuttle.Recall
{
    public class ProjectionProcessorFactory : IProcessorFactory
    {
        private readonly EventStoreOptions _eventStoreOptions;
        private readonly IPipelineFactory _pipelineFactory;
        private readonly IEventProcessor _eventProcessor;

        public ProjectionProcessorFactory(EventStoreOptions eventStoreOptions, IPipelineFactory pipelineFactory, IEventProcessor eventProcessor)
        {
            Guard.AgainstNull(eventStoreOptions,nameof(eventStoreOptions));
            Guard.AgainstNull(pipelineFactory, nameof(pipelineFactory));
            Guard.AgainstNull(eventProcessor, nameof(eventProcessor));

            _eventStoreOptions = eventStoreOptions;
            _pipelineFactory = pipelineFactory;
            _eventProcessor = eventProcessor;
        }

        public IProcessor Create()
        {
            return new ProjectionProcessor(_eventStoreOptions, _pipelineFactory, _eventProcessor);
        }
    }
}