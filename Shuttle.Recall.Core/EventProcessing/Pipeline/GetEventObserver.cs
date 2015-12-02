using Shuttle.Core.Infrastructure;

namespace Shuttle.Recall.Core
{
    public class GetEventObserver : IPipelineObserver<OnGetEvent>
    {
        public void Execute(OnGetEvent pipelineEvent)
        {
            var state = pipelineEvent.Pipeline.State;
            var reader = state.Get<IEventReader>();
            var position = state.Get<IEventProjectorPosition>();
            var projector = state.Get<IEventProjector>();

            var eventRead = reader.GetEvent(position.GetSequenceNumber(projector.Name));

            if (eventRead == null)
            {
                pipelineEvent.Pipeline.Abort();
            }
            else
            {
                state.SetWorking();
                state.Replace(eventRead);
            }
        }
    }
}