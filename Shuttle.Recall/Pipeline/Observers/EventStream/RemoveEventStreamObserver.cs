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
            ExecuteAsync(pipelineEvent, true).GetAwaiter().GetResult();
        }

        public async Task ExecuteAsync(OnRemoveEventStream pipelineEvent)
        {
            await ExecuteAsync(pipelineEvent, false).ConfigureAwait(false);
        }

        private async Task ExecuteAsync(OnRemoveEventStream pipelineEvent, bool sync)
        {
            var state = Guard.AgainstNull(pipelineEvent, nameof(pipelineEvent)).Pipeline.State;

            if (sync)
            {
                _primitiveEventRepository.Remove(state.GetId());
            }
            else
            {
                await _primitiveEventRepository.RemoveAsync(state.GetId()).ConfigureAwait(false);
            }
            
            await Task.CompletedTask;
        }
    }
}