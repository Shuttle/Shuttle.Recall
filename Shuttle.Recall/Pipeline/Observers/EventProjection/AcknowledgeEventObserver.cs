using System.Threading.Tasks;
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
            ExecuteAsync(pipelineEvent, true).GetAwaiter().GetResult();
        }

        public async Task ExecuteAsync(OnAcknowledgeEvent pipelineEvent)
        {
            await ExecuteAsync(pipelineEvent, false).ConfigureAwait(false);
        }

        private async Task ExecuteAsync(OnAcknowledgeEvent pipelineEvent, bool sync)
        {
            var state = Guard.AgainstNull(pipelineEvent, nameof(pipelineEvent)).Pipeline.State;
            var projection = Guard.AgainstNull(state.GetProjection(), StateKeys.Projection);
            var projectionEvent = Guard.AgainstNull(state.GetProjectionEvent(), nameof(ProjectionEvent));

            if (!projectionEvent.HasPrimitiveEvent)
            {
                return;
            }

            if (sync)
            {
                _repository.SetSequenceNumber(projection.Name, projection.SequenceNumber);
            }
            else
            {
                await _repository.SetSequenceNumberAsync(projection.Name, projection.SequenceNumber).ConfigureAwait(false);
            }
        }
    }
}