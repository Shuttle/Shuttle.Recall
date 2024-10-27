using System.IO;
using System.Threading.Tasks;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Serialization;

namespace Shuttle.Recall;

public interface IDeserializeEventEnvelopeObserver : IPipelineObserver<OnDeserializeEventEnvelope>
{
}

public class DeserializeEventEnvelopeObserver : IDeserializeEventEnvelopeObserver
{
    private readonly ISerializer _serializer;

    public DeserializeEventEnvelopeObserver(ISerializer serializer)
    {
        _serializer = Guard.AgainstNull(serializer);
    }

    public async Task ExecuteAsync(IPipelineContext<OnDeserializeEventEnvelope> pipelineContext)
    {
        var state = Guard.AgainstNull(pipelineContext).Pipeline.State;
        var primitiveEvent = state.GetPrimitiveEvent();

        EventEnvelope eventEnvelope;

        using (var stream = new MemoryStream(primitiveEvent.EventEnvelope))
        {
            eventEnvelope = (EventEnvelope)await _serializer.DeserializeAsync(typeof(EventEnvelope), stream).ConfigureAwait(false);
        }

        state.SetEventEnvelope(eventEnvelope);
        state.SetEventBytes(eventEnvelope.Event);

        eventEnvelope.AcceptInvariants();
    }
}