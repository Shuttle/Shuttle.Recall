using Shuttle.Core.Infrastructure;

namespace Shuttle.Recall.Core
{
    public class EventProjectionProcessor : IProcessor
    {
        private readonly ReusableObjectPool<EventProcessingPipeline> _pool = new ReusableObjectPool<EventProcessingPipeline>();
	    private readonly IEventProcessor _eventProcessor;
	    private readonly IEventProjection _eventProjection;
        private readonly IThreadActivity _threadActivity;

        public EventProjectionProcessor(IEventProcessor eventProcessor, IEventProjection eventProjection)
        {
            Guard.AgainstNull(eventProjection, "eventProjection");
            Guard.AgainstNull(eventProcessor, "eventProcessor");

	        _eventProcessor = eventProcessor;
	        _eventProjection = eventProjection;
			_eventProcessor = eventProcessor;
            _threadActivity = new ThreadActivity(_eventProcessor.Configuration.DurationToSleepWhenIdle);
        }

        public void Execute(IThreadState state)
        {
            while (state.Active)
            {
                var pipeline = _eventProcessor.Configuration.PipelineFactory.GetPipeline<EventProcessingPipeline>(_eventProcessor.Configuration);

                pipeline.State.Add(_eventProjection);
                pipeline.State.Add(_eventProcessor.Configuration.ProjectionEventReader);
                pipeline.State.Add(_eventProcessor.Configuration.ProjectionPosition);
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