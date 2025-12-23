using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Serialization;
using Shuttle.Core.Streams;

namespace Shuttle.Recall;

public interface ISerializeEventObserver : IPipelineObserver<SerializeEvent>;

public class SerializeEventObserver(ISerializer serializer) : ISerializeEventObserver
{
    private readonly ISerializer _serializer = Guard.AgainstNull(serializer);

    public async Task ExecuteAsync(IPipelineContext<SerializeEvent> pipelineContext, CancellationToken cancellationToken = default)
    {
        var state = Guard.AgainstNull(pipelineContext).Pipeline.State;
        var domainEvent = state.GetDomainEvent();
        var bytes = await (await _serializer.SerializeAsync(domainEvent.Event, cancellationToken).ConfigureAwait(false)).ToBytesAsync().ConfigureAwait(false);

        state.SetEventBytes(bytes);
    }
}