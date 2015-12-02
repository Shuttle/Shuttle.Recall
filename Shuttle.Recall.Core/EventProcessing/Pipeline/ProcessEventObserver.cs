using Shuttle.Core.Infrastructure;

namespace Shuttle.Recall.Core
{
    public class ProcessEventObserver : IPipelineObserver<OnProcessEvent>
    {
        public void Execute(OnProcessEvent pipelineEvent)
        {
            var state = pipelineEvent.Pipeline.State;
            var eventRead = state.Get<EventRead>();
            var projector = state.Get<IEventProjector>();

            if (!projector.HandlesType(eventRead.Event.Data.GetType()))
            {
                return;
            }

            projector.Process(eventRead, state.Get<IThreadState>());
        }
    }
}