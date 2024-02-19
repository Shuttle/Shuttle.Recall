using System.Threading.Tasks;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Recall
{
    public interface IRemoveEventStreamObserver : IPipelineObserver<OnRemoveEventStream>
    {
    }

    public class RemoveEventStreamObserver : IRemoveEventStreamObserver
    {
        private readonly IPrimitiveEventRepository _primitiveEventRepository;

        public RemoveEventStreamObserver(IPrimitiveEventRepository primitiveEventRepository)
        {
            _primitiveEventRepository = Guard.AgainstNull(primitiveEventRepository, nameof(primitiveEventRepository));
        }

        public void Execute(OnRemoveEventStream pipelineEvent)
        {
            ExecuteAsync(pipelineEvent).GetAwaiter().GetResult();
        }

        public async Task ExecuteAsync(OnRemoveEventStream pipelineEvent)
        {
            var state = Guard.AgainstNull(pipelineEvent, nameof(pipelineEvent)).Pipeline.State;

            _primitiveEventRepository.Remove(state.GetId());

            await Task.CompletedTask;
        }
    }
}