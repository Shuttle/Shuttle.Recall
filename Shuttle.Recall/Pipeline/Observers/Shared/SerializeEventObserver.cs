using Shuttle.Contract;
using Shuttle.Pipelines;
using Shuttle.Serialization;
using Shuttle.Streams;

namespace Shuttle.Recall;

public interface ISerializeEventObserver : IPipelineObserver<SerializeEvent>;

public class SerializeEventObserver(ISerializer serializer) : ISerializeEventObserver
{
    public async Task ExecuteAsync(IPipelineContext<SerializeEvent> pipelineContext, CancellationToken cancellationToken = default)
    {
        var state = Guard.AgainstNull(pipelineContext).Pipeline.State;
        var domainEvent = state.GetDomainEvent();
        var bytes = await (await serializer.SerializeAsync(domainEvent.Event, cancellationToken).ConfigureAwait(false)).ToBytesAsync().ConfigureAwait(false);

        state.SetEventBytes(bytes);
    }
}