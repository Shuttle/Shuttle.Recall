using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Serialization;

namespace Shuttle.Recall;

public interface IDeserializeEventObserver : IPipelineObserver<DeserializeEvent>;

public class DeserializeEventObserver(ISerializer serializer) : IDeserializeEventObserver
{
    private readonly ISerializer _serializer = Guard.AgainstNull(serializer);

    public async Task ExecuteAsync(IPipelineContext<DeserializeEvent> pipelineContext, CancellationToken cancellationToken = default)
    {
        var state = Guard.AgainstNull(pipelineContext).Pipeline.State;
        var eventEnvelope = state.GetEventEnvelope();

        using var stream = new MemoryStream(eventEnvelope.Event);

        state.SetDomainEvent(new(await _serializer.DeserializeAsync(Guard.AgainstNull(Type.GetType(Guard.AgainstEmpty(eventEnvelope.AssemblyQualifiedName), true, true)), stream, cancellationToken), eventEnvelope.Version));
    }
}