using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Threading;

namespace Shuttle.Recall
{
    public class ProjectionProcessorFactory : IProcessorFactory
    {
        private readonly EventStoreOptions _eventStoreOptions;
        private readonly IPipelineFactory _pipelineFactory;
        private readonly EventProcessor _eventProcessor;

        public ProjectionProcessorFactory(EventStoreOptions eventStoreOptions, IPipelineFactory pipelineFactory, EventProcessor eventProcessor)
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