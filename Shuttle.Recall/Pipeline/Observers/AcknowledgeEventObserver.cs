using Shuttle.Core.Infrastructure;

namespace Shuttle.Recall
{
    public class AcknowledgeEventObserver : IPipelineObserver<OnAcknowledgeEvent>
    {
        private readonly IProjectionRepository _repository;
        private readonly IProjectionSequenceNumberTracker _tracker;

        public AcknowledgeEventObserver(IProjectionRepository repository, IProjectionSequenceNumberTracker tracker)
        {
            Guard.AgainstNull(repository, "repository");
            Guard.AgainstNull(tracker, "tracker");

            _repository = repository;
            _tracker = tracker;
        }

        public void Execute(OnAcknowledgeEvent pipelineEvent)
        {
            var state = pipelineEvent.Pipeline.State;
            var rawEvent = state.GetPrimitiveEvent();
            var projection = state.GetProjection();

            _tracker.Set(projection.Name, rawEvent.SequenceNumber + 1);
            _repository.SetSequenceNumber(projection.Name, rawEvent.SequenceNumber + 1);
        }
    }
}