using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Recall
{
    public interface IAcknowledgeEventObserver : IPipelineObserver<OnAcknowledgeEvent>
    {
    }

    public class AcknowledgeEventObserver : IAcknowledgeEventObserver
    {
        private readonly IProjectionRepository _repository;

        public AcknowledgeEventObserver(IProjectionRepository repository)
        {
            Guard.AgainstNull(repository, nameof(repository));

            _repository = repository;
        }

        public void Execute(OnAcknowledgeEvent pipelineEvent)
        {
            var state = pipelineEvent.Pipeline.State;
            var projection = state.GetProjection();

            _repository.SetSequenceNumber(projection.Name, projection.SequenceNumber);
        }
    }
}