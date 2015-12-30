using Shuttle.Core.Infrastructure;

namespace Shuttle.Recall.Core
{
    public class EventProjectionProcessor : IProcessor
    {
        private readonly ReusableObjectPool<EventProcessingPipeline> _pool = new ReusableObjectPool<EventProcessingPipeline>();
        private readonly IEventProjection _eventProjection;
        private readonly IEventProcessorConfiguration _configuration;
        private readonly IThreadActivity _threadActivity;

        public EventProjectionProcessor(IEventProcessorConfiguration configuration, IEventProjection eventProjection)
        {
            Guard.AgainstNull(eventProjection, "eventProjection");
            Guard.AgainstNull(configuration, "configuration");

            _eventProjection = eventProjection;
            _configuration = configuration;
            _threadActivity = new ThreadActivity(configuration.DurationToSleepWhenIdle);
        }

        public void Execute(IThreadState state)
        {
            while (state.Active)
            {
                var pipeline = _configuration.PipelineFactory.GetPipeline<EventProcessingPipeline>(_configuration);

                pipeline.State.Add(_eventProjection);
                pipeline.State.Add(_configuration.ProjectionEventReader);
                pipeline.State.Add(_configuration.ProjectionPosition);
                pipeline.State.Add(state);

                try
                {
                    pipeline.Execute();
                }
                finally
                {
                    _pool.Release(pipeline);
                }

                if (pipeline.State.GetWorking())
                {
                    _threadActivity.Working();
                }
                else
                {
                    _threadActivity.Waiting(state);
                }
            }
        }
    }
}