using Shuttle.Core.Infrastructure;

namespace Shuttle.Recall.Core
{
    public class EventProjectorProcessor : IProcessor
    {
        private readonly ReusableObjectPool<EventProcessingPipeline> _pool = new ReusableObjectPool<EventProcessingPipeline>();
        private readonly IEventProjector _eventProjector;
        private readonly IEventProcessorConfiguration _configuration;
        private readonly IThreadActivity _threadActivity;

        public EventProjectorProcessor(IEventProcessorConfiguration configuration, IEventProjector eventProjector)
        {
            Guard.AgainstNull(eventProjector, "eventProjector");
            Guard.AgainstNull(configuration, "configuration");

            _eventProjector = eventProjector;
            _configuration = configuration;
            _threadActivity = new ThreadActivity(configuration.DurationToSleepWhenIdle);
        }

        public void Execute(IThreadState state)
        {
            while (state.Active)
            {
                var pipeline = _configuration.PipelineFactory.GetPipeline<EventProcessingPipeline>(_configuration);

                pipeline.State.Add(_eventProjector);
                pipeline.State.Add(_configuration.EventReader);
                pipeline.State.Add(_configuration.EventProjectorPosition);
                pipeline.State.Add(state);

                try
                {
                    pipeline.Execute();
                }
                finally
                {
                    _pool.Release(pipeline);
                }

                _threadActivity.Waiting(state);
            }
        }
    }
}