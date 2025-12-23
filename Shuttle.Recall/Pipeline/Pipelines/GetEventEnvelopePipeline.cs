using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Recall;

public class GetEventEnvelopePipeline : Pipeline
{
    public GetEventEnvelopePipeline(IPipelineDependencies pipelineDependencies, IDeserializeEventEnvelopeObserver deserializeEventEnvelopeObserver, IDecompressEventObserver decompressEventObserver, IDecryptEventObserver decryptEventObserver, IDeserializeEventObserver deserializeEventObserver)
        : base(pipelineDependencies)
    {
        AddStage("GetEventEnvelope")
            .WithEvent<DeserializeEventEnvelope>()
            .WithEvent<EventEnvelopeDeserialized>()
            .WithEvent<DecompressEvent>()
            .WithEvent<EventDecompressed>()
            .WithEvent<DecryptEvent>()
            .WithEvent<EventDecrypted>()
            .WithEvent<DeserializeEvent>()
            .WithEvent<EventDeserialized>();

        AddObserver(Guard.AgainstNull(deserializeEventEnvelopeObserver));
        AddObserver(Guard.AgainstNull(decompressEventObserver));
        AddObserver(Guard.AgainstNull(decryptEventObserver));
        AddObserver(Guard.AgainstNull(deserializeEventObserver));
    }

    public async Task ExecuteAsync(PrimitiveEvent? primitiveEvent)
    {
        Guard.AgainstNull(primitiveEvent);

        State.SetPrimitiveEvent(primitiveEvent);

        await ExecuteAsync().ConfigureAwait(false);
    }
}