using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Recall
{
    public class GetEventEnvelopePipeline : Pipeline
    {
        public GetEventEnvelopePipeline(IDeserializeEventEnvelopeObserver deserializeEventEnvelopeObserver,
            IDecompressEventObserver decompressEventObserver, IDecryptEventObserver decryptEventObserver,
            IDeserializeEventObserver deserializeEventObserver)
        {
            Guard.AgainstNull(deserializeEventEnvelopeObserver, nameof(deserializeEventEnvelopeObserver));
            Guard.AgainstNull(decompressEventObserver, nameof(decompressEventObserver));
            Guard.AgainstNull(decryptEventObserver, nameof(decryptEventObserver));
            Guard.AgainstNull(deserializeEventObserver, nameof(deserializeEventObserver));

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

        public void Execute(PrimitiveEvent primitiveEvent)
        {
            Guard.AgainstNull(primitiveEvent, nameof(primitiveEvent));

            State.SetPrimitiveEvent(primitiveEvent);

            Execute();
        }
    }
}