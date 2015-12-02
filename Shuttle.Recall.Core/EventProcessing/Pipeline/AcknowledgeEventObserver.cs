using Shuttle.Core.Infrastructure;

namespace Shuttle.Recall.Core
{
    public class AcknowledgeEventObserver : IPipelineObserver<OnAcknowledgeEvent>
    {
        public void Execute(OnAcknowledgeEvent pipelineEvent)
        {
            var state = pipelineEvent.Pipeline.State;
            var position = state.Get<IEventProjectorPosition>();
            var eventRead = state.Get<EventRead>();
            var projector = state.Get<IEventProjector>();

            position.SetSequenceNumber(projector.Name, eventRead.SequenceNumber + 1);
        }
    }
}