using Shuttle.Core.Infrastructure;

namespace Shuttle.Recall
{
    public class ProjectionPrimitiveEventObserver : IPipelineObserver<OnGetProjectionPrimitiveEvent>
    {
        private readonly IEventStoreConfiguration _configuration;
        private readonly IPrimitiveEventRepository _repository;
        private readonly IPrimitiveEventQueue _queue;

        public ProjectionPrimitiveEventObserver(IEventStoreConfiguration configuration, IPrimitiveEventRepository repository, IPrimitiveEventQueue queue)
        {
            Guard.AgainstNull(configuration, "configuration");
            Guard.AgainstNull(repository, "repository");
            Guard.AgainstNull(queue, "queue");

            _configuration = configuration;
            _repository = repository;
            _queue = queue;
        }

        public void Execute(OnGetProjectionPrimitiveEvent pipelineEvent)
        {
            var state = pipelineEvent.Pipeline.State;
            var projection = state.GetProjection();
            var sequenceNumber = state.GetSequenceNumber();

            var primitiveEvent = _queue.Dequeue(projection.Name);

            if (primitiveEvent == null)
            {
                _queue.EnqueueRange(projection.Name, _repository.Get(sequenceNumber, projection.EventTypes, _configuration.ProjectionEventFetchCount));

                primitiveEvent = _queue.Dequeue(projection.Name);
            }

            if (primitiveEvent == null)
            {
                pipelineEvent.Pipeline.Abort();
            }
            else
            {
                state.SetWorking();
                state.SetPrimitiveEvent(primitiveEvent);
            }
        }
    }
}