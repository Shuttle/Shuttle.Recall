using Shuttle.Core.Infrastructure;

namespace Shuttle.Recall
{
    public class EventProjectionProcessor : IProcessor
    {
        private readonly IPipelineFactory _pipelineFactory;
        private readonly EventProjection _eventProjection;
        private readonly IThreadActivity _threadActivity;

        public EventProjectionProcessor(IEventStoreConfiguration configuration, IPipelineFactory pipelineFactory, EventProjection eventProjection)
        {
            Guard.AgainstNull(configuration, "configuration");
            Guard.AgainstNull(pipelineFactory, "pipelineFactory");
            Guard.AgainstNull(eventProjection, "eventProjection");

            _pipelineFactory = pipelineFactory;
            _eventProjection = eventProjection;
            _threadActivity = new ThreadActivity(configuration.DurationToSleepWhenIdle);
        }

        public void Execute(IThreadState state)
        {
            var pipeline = _pipelineFactory.GetPipeline<EventProcessingPipeline>();

            while (state.Active)
            {
                pipeline.State.SetEventProjection(_eventProjection);
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