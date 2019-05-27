using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Threading;

namespace Shuttle.Recall
{
    public class ProjectionProcessorFactory : IProcessorFactory
    {
        private readonly IEventStoreConfiguration _configuration;
        private readonly IPipelineFactory _pipelineFactory;
        private readonly EventProcessor _eventProcessor;

        public ProjectionProcessorFactory(IEventStoreConfiguration configuration, IPipelineFactory pipelineFactory, EventProcessor eventProcessor)
        {
            Guard.AgainstNull(configuration,nameof(configuration));
            Guard.AgainstNull(pipelineFactory, nameof(pipelineFactory));
            Guard.AgainstNull(eventProcessor, nameof(eventProcessor));

            _configuration = configuration;
            _pipelineFactory = pipelineFactory;
            _eventProcessor = eventProcessor;
        }

        public IProcessor Create()
        {
            return new ProjectionProcessor(_configuration, _pipelineFactory, _eventProcessor);
        }
    }
}