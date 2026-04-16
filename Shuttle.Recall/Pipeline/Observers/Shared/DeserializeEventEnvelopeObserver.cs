using Shuttle.Contract;
using Shuttle.Pipelines;
using Shuttle.Serialization;

namespace Shuttle.Recall;

public interface IDeserializeEventEnvelopeObserver : IPipelineObserver<DeserializeEventEnvelope>;

public class DeserializeEventEnvelopeObserver(ISerializer serializer) : IDeserializeEventEnvelopeObserver
{
    private readonly ISerializer _serializer = Guard.AgainstNull(serializer);

    public async Task ExecuteAsync(IPipelineContext<DeserializeEventEnvelope> pipelineContext, CancellationToken cancellationToken = default)
    {
        var state = Guard.AgainstNull(pipelineContext).Pipeline.State;
        var primitiveEvent = state.GetPrimitiveEvent();

        EventEnvelope eventEnvelope;

        using (var stream = new MemoryStream(primitiveEvent.EventEnvelope))
        {
            eventEnvelope = (EventEnvelope)await _serializer.DeserializeAsync(typeof(EventEnvelope), stream, cancellationToken).ConfigureAwait(false);
        }

        state.SetEventEnvelope(eventEnvelope);
        state.SetEventBytes(eventEnvelope.Event);

        eventEnvelope.AcceptInvariants();
    }
}