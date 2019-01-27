using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Threading;

namespace Shuttle.Recall
{
    public class ProjectionProcessor : IProcessor
    {
        private readonly IPipelineFactory _pipelineFactory;
        private readonly IEventProcessor _eventProcessor;
        private readonly IThreadActivity _threadActivity;

        public ProjectionProcessor(IEventStoreConfiguration configuration, IPipelineFactory pipelineFactory,
            IEventProcessor eventProcessor)
        {
            Guard.AgainstNull(configuration, nameof(configuration));
            Guard.AgainstNull(pipelineFactory, nameof(pipelineFactory));
            Guard.AgainstNull(eventProcessor, nameof(eventProcessor));

            _pipelineFactory = pipelineFactory;
            _eventProcessor = eventProcessor;
            _threadActivity = new ThreadActivity(configuration.DurationToSleepWhenIdle);
        }

        public void Execute(IThreadState state)
        {
            var pipeline = _pipelineFactory.GetPipeline<EventProcessingPipeline>();

            while (state.Active)
            {
                var projection = _eventProcessor.GetProjection();
                var waiting = true;

                if (projection != null)
                {
                    pipeline.State.Clear();
                    pipeline.State.SetProjection(projection);
                    pipeline.State.SetThreadState(state);

                    pipeline.Execute();

                    if (pipeline.State.GetWorking())
                    {
                        _threadActivity.Working();
                        waiting = false;
                    }

                    _eventProcessor.ReleaseProjection(projection.Name);
                }

                if (!waiting)
                {
                    _threadActivity.Waiting(state);
                }
            }

            _pipelineFactory.ReleasePipeline(pipeline);
        }
    }
}