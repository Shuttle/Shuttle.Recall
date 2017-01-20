using Shuttle.Core.Infrastructure;

namespace Shuttle.Recall
{
    public class ProjectionEventEnvelopeObserver : IPipelineObserver<OnGetProjectionEventEnvelope>
    {
        private readonly IPipelineFactory _pipelineFactory;

        public ProjectionEventEnvelopeObserver(IPipelineFactory pipelineFactory)
        {
            Guard.AgainstNull(pipelineFactory, "pipelineFactory");

            _pipelineFactory = pipelineFactory;
        }

        public void Execute(OnGetProjectionEventEnvelope pipelineEvent)
        {
            var state = pipelineEvent.Pipeline.State;
            var primitiveEvent = state.GetPrimitiveEvent();

            Guard.AgainstNull(primitiveEvent, "state.GetPrimitiveEvent()");

            var pipeline = _pipelineFactory.GetPipeline<GetEventEnvelopePipeline>();

            try
            {
                pipeline.Execute(primitiveEvent);

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