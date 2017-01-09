using Shuttle.Core.Infrastructure;

namespace Shuttle.Recall
{
    public class EventStreamObserver : IPipelineObserver<OnCommitEventStream>
    {
        public void Execute(OnCommitEventStream pipelineEvent)
        {
            var state = pipelineEvent.Pipeline.State;
            var eventStream = state.GetEventStream();

            Guard.AgainstNull(eventStream, "state.GetEventStream()");

            eventStream.Commit();
        }
    }
}