using System.Threading.Tasks;
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
            ExecuteAsync(pipelineEvent, true).GetAwaiter().GetResult();
        }

        public async Task ExecuteAsync(OnGetProjectionEventEnvelope pipelineEvent)
        {
            await ExecuteAsync(pipelineEvent, false).ConfigureAwait(false);
        }

        public async Task ExecuteAsync(OnGetProjectionEventEnvelope pipelineEvent, bool sync)
        {
            var state = Guard.AgainstNull(pipelineEvent, nameof(pipelineEvent)).Pipeline.State;
            var projectionEvent = Guard.AgainstNull(state.GetProjectionEvent(), StateKeys.ProjectionEvent);

            if (!projectionEvent.HasPrimitiveEvent)
            {
                return;
            }

            var pipeline = _pipelineFactory.GetPipeline<GetEventEnvelopePipeline>();

            try
            {
                if (sync)
                {
                    pipeline.Execute(projectionEvent.PrimitiveEvent);
                }
                else
                {
                    await pipeline.ExecuteAsync(projectionEvent.PrimitiveEvent);
                }

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