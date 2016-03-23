using Shuttle.Core.Infrastructure;

namespace Shuttle.Recall
{
    public class AcknowledgeEventObserver : IPipelineObserver<OnAcknowledgeEvent>
    {
        public void Execute(OnAcknowledgeEvent pipelineEvent)
        {
            var state = pipelineEvent.Pipeline.State;
            var position = state.Get<IProjectionService>();
            var eventRead = state.Get<ProjectionEvent>();
            var projection = state.Get<IEventProjection>();

            position.SetSequenceNumber(projection.Name, eventRead.SequenceNumber + 1);
        }
    }
}