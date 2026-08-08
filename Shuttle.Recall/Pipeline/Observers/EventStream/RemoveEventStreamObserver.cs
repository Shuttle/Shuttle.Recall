using Shuttle.Contract;
using Shuttle.Pipelines;

namespace Shuttle.Recall;

public interface IRemoveEventStreamObserver : IPipelineObserver<RemoveEventStream>;

public class RemoveEventStreamObserver(IPrimitiveEventRepository primitiveEventRepository) : IRemoveEventStreamObserver
{
    public async Task ExecuteAsync(IPipelineContext<RemoveEventStream> pipelineContext, CancellationToken cancellationToken = default)
    {
        await primitiveEventRepository.RemoveAsync(new PrimitiveEvent.Specification().AddId(Guard.AgainstNull(pipelineContext).Pipeline.State.GetId()), cancellationToken).ConfigureAwait(false);
    }
}