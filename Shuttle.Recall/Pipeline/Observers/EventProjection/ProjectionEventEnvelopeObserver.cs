using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Recall
{
    public interface IProjectionEventEnvelopeObserver : IPipelineObserver<OnGetProjectionEventEnvelope>
    {
    }

    public class ProjectionEventEnvelopeObserver : IProjectionEventEnvelopeObserver
    {
        private readonly IPipelineFactory _pipelineFactory;

        public ProjectionEventEnvelopeObserver(IPipelineFactory pipelineFactory)
        {
            Guard.AgainstNull(pipelineFactory, nameof(pipelineFactory));

            _pipelineFactory = pipelineFactory;
        }

        public void Execute(OnGetProjectionEventEnvelope pipelineEvent)
        {
            var state = pipelineEvent.Pipeline.State;
            var projectionEvent = state.GetProjectionEvent();

            Guard.AgainstNull(projectionEvent, nameof(projectionEvent));

            if (!projectionEvent.HasPrimitiveEvent)
            {
                return;
            }

            var pipeline = _pipelineFactory.GetPipeline<GetEventEnvelopePipeline>();

            try
            {
                pipeline.Execute(projectionEvent.PrimitiveEvent);

                state.SetEventEnvelope(pipeline.State.GetEventEnvelope());
                state.SetEvent(pipeline.State.GetEvent());
            }
            finally
            {
                _pipelineFactory.ReleasePipeline(pipeline);
            }
        }
    }
}