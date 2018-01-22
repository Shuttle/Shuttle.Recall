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
            Guard.AgainstNull(primitiveEventRepository, nameof(primitiveEventRepository));

            _primitiveEventRepository = primitiveEventRepository;
        }

        public void Execute(OnRemoveEventStream pipelineEvent)
        {
            var state = pipelineEvent.Pipeline.State;

            _primitiveEventRepository.Remove(state.GetId());
        }
    }
}