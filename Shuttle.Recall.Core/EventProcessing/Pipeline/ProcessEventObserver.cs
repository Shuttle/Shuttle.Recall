using Shuttle.Core.Infrastructure;

namespace Shuttle.Recall.Core
{
    public class ProcessEventObserver : IPipelineObserver<OnProcessEvent>
    {
        public void Execute(OnProcessEvent pipelineEvent)
        {
            var state = pipelineEvent.Pipeline.State;
            var eventRead = state.Get<ProjectionEvent>();
            var projection = state.Get<IEventProjection>();

            if (!projection.HandlesType(eventRead.Event.Data.GetType()))
            {
                return;
            }

            projection.Process(eventRead, state.Get<IThreadState>());
        }
    }
}