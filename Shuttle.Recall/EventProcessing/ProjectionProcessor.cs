using Shuttle.Core.Infrastructure;

namespace Shuttle.Recall
{
    public class ProjectionProcessor : IProcessor
    {
        private readonly IPipelineFactory _pipelineFactory;
        private readonly Projection _projection;
        private readonly IThreadActivity _threadActivity;

        public ProjectionProcessor(IEventStoreConfiguration configuration, IPipelineFactory pipelineFactory, Projection projection)
        {
            Guard.AgainstNull(configuration, "configuration");
            Guard.AgainstNull(pipelineFactory, "pipelineFactory");
            Guard.AgainstNull(projection, "Projection");

            _pipelineFactory = pipelineFactory;
            _projection = projection;
            _threadActivity = new ThreadActivity(configuration.DurationToSleepWhenIdle);
        }

        public void Execute(IThreadState state)
        {
            var pipeline = _pipelineFactory.GetPipeline<EventProcessingPipeline>();

            while (state.Active)
            {
                pipeline.State.Clear();
                pipeline.State.SetProjection(_projection);
                pipeline.State.SetThreadState(state);

                pipeline.Execute();

                if (pipeline.State.GetWorking())
                {
                    _threadActivity.Working();
                }
                else
                {
                    _threadActivity.Waiting(state);
                }
            }

            _pipelineFactory.ReleasePipeline(pipeline);
        }
    }
}