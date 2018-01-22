using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Recall
{
    public interface IProjectionPrimitiveEventObserver : IPipelineObserver<OnGetProjectionPrimitiveEvent>
    {
    }

    public class ProjectionPrimitiveEventObserver : IProjectionPrimitiveEventObserver
    {
        private readonly IEventStoreConfiguration _configuration;
        private readonly IPrimitiveEventQueue _queue;
        private readonly IPrimitiveEventRepository _repository;

        public ProjectionPrimitiveEventObserver(IEventStoreConfiguration configuration,
            IPrimitiveEventRepository repository, IPrimitiveEventQueue queue)
        {
            Guard.AgainstNull(configuration, nameof(configuration));
            Guard.AgainstNull(repository, nameof(repository));
            Guard.AgainstNull(queue, nameof(queue));

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
                _queue.EnqueueRange(projection.Name,
                    _repository.Get(sequenceNumber, projection.EventTypes, _configuration.ProjectionEventFetchCount));

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