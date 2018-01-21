using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Recall
{
    public class EventStreamObserver : IPipelineObserver<OnCommitEventStream>
    {
        public void Execute(OnCommitEventStream pipelineEvent)
        {
            var state = pipelineEvent.Pipeline.State;
            var eventStream = state.GetEventStream();

            Guard.AgainstNull(eventStream, nameof(eventStream));

            eventStream.Commit();
        }
    }
}