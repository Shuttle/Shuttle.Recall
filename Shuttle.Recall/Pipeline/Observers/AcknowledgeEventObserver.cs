using Shuttle.Core.Infrastructure;

namespace Shuttle.Recall
{
    public class AcknowledgeEventObserver : IPipelineObserver<OnAcknowledgeEvent>
    {
        private readonly IProjectionRepository _repository;

        public AcknowledgeEventObserver(IProjectionRepository repository)
        {
            Guard.AgainstNull(repository, "repository");

            _repository = repository;
        }

        public void Execute(OnAcknowledgeEvent pipelineEvent)
        {
            var state = pipelineEvent.Pipeline.State;
            var rawEvent = state.GetPrimitiveEvent();
            var projection = state.GetProjection();

            _repository.SetSequenceNumber(projection.Name, rawEvent.SequenceNumber + 1);
        }
    }
}