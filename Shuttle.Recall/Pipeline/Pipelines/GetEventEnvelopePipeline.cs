using Shuttle.Core.Infrastructure;
using Shuttle.Recall.Shared;

namespace Shuttle.Recall
{
    public class GetEventEnvelopePipeline : Pipeline
    {
        public GetEventEnvelopePipeline(DeserializeEventEnvelopeObserver deserializeEventEnvelopeObserver, DecompressEventObserver decompressEventObserver, DecryptEventObserver decryptEventObserver, DeserializeEventObserver deserializeEventObserver)
        {
            Guard.AgainstNull(deserializeEventEnvelopeObserver, "deserializeEventEnvelopeObserver");
            Guard.AgainstNull(decompressEventObserver, "decompressEventObserver");
            Guard.AgainstNull(decryptEventObserver, "decryptEventObserver");
            Guard.AgainstNull(deserializeEventObserver, "deserializeEventObserver");

            RegisterStage("Get")
                .WithEvent<OnDeserializeEventEnvelope>()
                .WithEvent<OnAfterDeserializeEventEnvelope>()
                .WithEvent<OnDecompressEvent>()
                .WithEvent<OnAfterDecompressEvent>()
                .WithEvent<OnDecryptEvent>()
                .WithEvent<OnAfterDecryptEvent>()
                .WithEvent<OnDeserializeEvent>()
                .WithEvent<OnAfterDeserializeEvent>();

            RegisterObserver(deserializeEventEnvelopeObserver);
            RegisterObserver(decompressEventObserver);
            RegisterObserver(decryptEventObserver);
            RegisterObserver(deserializeEventObserver);
        }

        public EventEnvelope Execute(PrimitiveEvent primitiveEvent)
        {
            Guard.AgainstNull(primitiveEvent, "primitiveEvent");

            State.SetPrimitiveEvent(primitiveEvent);

            Execute();

            return State.GetEventEnvelope();
        }
    }
}