using System.Threading.Tasks;
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
            ExecuteAsync(pipelineEvent).GetAwaiter().GetResult();
        }

        public async Task ExecuteAsync(OnCommitEventStream pipelineEvent)
        {
            var state = Guard.AgainstNull(pipelineEvent, nameof(pipelineEvent)).Pipeline.State;
            var eventStream = state.GetEventStream();

            Guard.AgainstNull(eventStream, nameof(eventStream));

            eventStream.Commit();

            await Task.CompletedTask;
        }
    }
}