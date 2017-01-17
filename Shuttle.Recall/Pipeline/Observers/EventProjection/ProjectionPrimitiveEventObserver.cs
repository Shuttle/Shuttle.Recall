using Shuttle.Core.Infrastructure;

namespace Shuttle.Recall
{
    public class ProjectionPrimitiveEventObserver : IPipelineObserver<OnGetProjectionPrimitiveEvent>
    {
        private readonly IEventStoreConfiguration _configuration;
        private readonly IPrimitiveEventRepository _repository;
        private readonly IPrimitiveEventCache _cache;

        public ProjectionPrimitiveEventObserver(IEventStoreConfiguration configuration, IPrimitiveEventRepository repository, IPrimitiveEventCache cache)
        {
            Guard.AgainstNull(configuration, "configuration");
            Guard.AgainstNull(repository, "repository");
            Guard.AgainstNull(cache, "cache");

            _configuration = configuration;
            _repository = repository;
            _cache = cache;
        }

        public void Execute(OnGetProjectionPrimitiveEvent pipelineEvent)
        {
            var state = pipelineEvent.Pipeline.State;
            var projection = state.GetProjection();
            var sequenceNumber = state.GetSequenceNumber();

            var primitiveEvent = _cache.TryGet(sequenceNumber);

            if (primitiveEvent == null)
            {
                _cache.AddRange(_repository.Get(sequenceNumber, projection.EventTypes, _configuration.ProjectionEventFetchCount));
            }

            primitiveEvent = _cache.TryGet(sequenceNumber);

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