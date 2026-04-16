using Shuttle.Contract;
using Shuttle.Pipelines;

namespace Shuttle.Recall;

public interface IRemoveEventStreamObserver : IPipelineObserver<RemoveEventStream>;

public class RemoveEventStreamObserver(IPrimitiveEventRepository primitiveEventRepository) : IRemoveEventStreamObserver
{
    private readonly IPrimitiveEventRepository _primitiveEventRepository = Guard.AgainstNull(primitiveEventRepository);

    public async Task ExecuteAsync(IPipelineContext<RemoveEventStream> pipelineContext, CancellationToken cancellationToken = default)
    {
        await _primitiveEventRepository.RemoveAsync(new PrimitiveEvent.Specification().AddId(Guard.AgainstNull(pipelineContext).Pipeline.State.GetId()), cancellationToken).ConfigureAwait(false);
    }
}