using Shuttle.Core.Infrastructure;

namespace Shuttle.Recall
{
    public class AcknowledgeEventObserver : IPipelineObserver<OnAcknowledgeEvent>
    {
        private readonly IEventProjectionRepository _repository;

        public AcknowledgeEventObserver(IEventProjectionRepository repository)
        {
            Guard.AgainstNull(repository, "repository");

            _repository = repository;
        }

        public void Execute(OnAcknowledgeEvent pipelineEvent)
        {
            var state = pipelineEvent.Pipeline.State;
            var rawEvent = state.GetPrimitiveEvent();
            var projection = state.GetEventProjection();

            _repository.SetSequenceNumber(projection.Name, rawEvent.SequenceNumber + 1);
        }
    }
}