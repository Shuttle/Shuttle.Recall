using Shuttle.Core.Infrastructure;

namespace Shuttle.Recall.Core
{
    public class GetEventObserver : IPipelineObserver<OnGetEvent>
    {
        public void Execute(OnGetEvent pipelineEvent)
        {
            var state = pipelineEvent.Pipeline.State;
            var reader = state.Get<IProjectionEventReader>();
            var position = state.Get<IProjectionPosition>();
            var projection = state.Get<IEventProjection>();

            var eventRead = reader.GetEvent(position.GetSequenceNumber(projection.Name));

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