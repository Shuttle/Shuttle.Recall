using Shuttle.Core.Infrastructure;

namespace Shuttle.Recall
{
    public class ProcessEventObserver : IPipelineObserver<OnProcessEvent>
    {
        private readonly ILog _log;

        public ProcessEventObserver()
        {
            _log = Log.For(this);
        }

        public void Execute(OnProcessEvent pipelineEvent)
        {
            var state = pipelineEvent.Pipeline.State;
            var eventRead = state.Get<ProjectionEvent>();
            var projection = state.Get<IEventProjection>();

            if (!projection.HandlesType(eventRead.Event.Data.GetType()))
            {
                if (Log.IsTraceEnabled)
                {
                    _log.Trace(string.Format(RecallResources.TraceTypeNotHandled, projection.Name, eventRead.Event.Data.GetType()));
                }

                return;
            }

            projection.Process(eventRead, state.Get<IThreadState>());
        }
    }
}