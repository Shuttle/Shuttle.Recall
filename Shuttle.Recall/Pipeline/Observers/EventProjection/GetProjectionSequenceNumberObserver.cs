using Shuttle.Core.Infrastructure;

namespace Shuttle.Recall
{
    public class GetProjectionSequenceNumberObserver : IPipelineObserver<OnGetProjectionSequenceNumber>
    {
        private readonly IProjectionRepository _projectionRepository;
        private readonly IProjectionSequenceNumberTracker _tracker;

        public GetProjectionSequenceNumberObserver(IProjectionRepository projectionRepository,
            IProjectionSequenceNumberTracker tracker)
        {
            Guard.AgainstNull(projectionRepository, "projectionRepository");
            Guard.AgainstNull(tracker, "tracker");

            _projectionRepository = projectionRepository;
            _tracker = tracker;
        }

        public void Execute(OnGetProjectionSequenceNumber pipelineEvent)
        {
            var state = pipelineEvent.Pipeline.State;
            var projection = state.GetProjection();

            Guard.AgainstNull(projection, "state.GetProjection()");

            state.SetSequenceNumber(_tracker.TryGet(projection.Name) ??
                                    _projectionRepository.GetSequenceNumber(projection.Name));
        }
    }
}