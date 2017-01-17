using Shuttle.Core.Infrastructure;

namespace Shuttle.Recall
{
    public class RemoveEventStreamObserver : IPipelineObserver<OnRemoveEventStream>
    {
        private readonly IPrimitiveEventRepository _primitiveEventRepository;

        public RemoveEventStreamObserver(IPrimitiveEventRepository primitiveEventRepository)
        {
            Guard.AgainstNull(primitiveEventRepository, "primitiveEventRepository");

            _primitiveEventRepository = primitiveEventRepository;
        }

        public void Execute(OnRemoveEventStream pipelineEvent)
        {
            var state = pipelineEvent.Pipeline.State;

            _primitiveEventRepository.Remove(state.GetId());
        }
    }
}