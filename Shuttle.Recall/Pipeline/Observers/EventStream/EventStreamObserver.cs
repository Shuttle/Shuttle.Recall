using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Recall
{
    public interface IEventStreamObserver : IPipelineObserver<OnCommitEventStream>
    {
    }

    public class EventStreamObserver : IEventStreamObserver
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