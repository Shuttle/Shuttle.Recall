using System.Threading.Tasks;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Recall;

public interface IRemoveEventStreamObserver : IPipelineObserver<OnRemoveEventStream>
{
}

public class RemoveEventStreamObserver : IRemoveEventStreamObserver
{
    private readonly IPrimitiveEventRepository _primitiveEventRepository;

    public RemoveEventStreamObserver(IPrimitiveEventRepository primitiveEventRepository)
    {
        _primitiveEventRepository = Guard.AgainstNull(primitiveEventRepository);
    }

    public async Task ExecuteAsync(IPipelineContext<OnRemoveEventStream> pipelineContext)
    {
        await _primitiveEventRepository.RemoveAsync(Guard.AgainstNull(pipelineContext).Pipeline.State.GetId()).ConfigureAwait(false);
    }
}