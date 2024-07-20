using System.Threading.Tasks;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Recall
{
    public class GetEventEnvelopePipeline : Pipeline
    {
        public GetEventEnvelopePipeline(IDeserializeEventEnvelopeObserver deserializeEventEnvelopeObserver, IDecompressEventObserver decompressEventObserver, IDecryptEventObserver decryptEventObserver, IDeserializeEventObserver deserializeEventObserver) {
            RegisterStage("Get")
                .WithEvent<OnDeserializeEventEnvelope>()
                .WithEvent<OnAfterDeserializeEventEnvelope>()
                .WithEvent<OnDecompressEvent>()
                .WithEvent<OnAfterDecompressEvent>()
                .WithEvent<OnDecryptEvent>()
                .WithEvent<OnAfterDecryptEvent>()
                .WithEvent<OnDeserializeEvent>()
                .WithEvent<OnAfterDeserializeEvent>();

            RegisterObserver(Guard.AgainstNull(deserializeEventEnvelopeObserver, nameof(deserializeEventEnvelopeObserver)));
            RegisterObserver(Guard.AgainstNull(decompressEventObserver, nameof(decompressEventObserver)));
            RegisterObserver(Guard.AgainstNull(decryptEventObserver, nameof(decryptEventObserver)));
            RegisterObserver(Guard.AgainstNull(deserializeEventObserver, nameof(deserializeEventObserver)));
        }

        public void Execute(PrimitiveEvent primitiveEvent)
        {
            ExecuteAsync(primitiveEvent).GetAwaiter().GetResult();
        }

        public async Task ExecuteAsync(PrimitiveEvent primitiveEvent)
        {
            Guard.AgainstNull(primitiveEvent, nameof(primitiveEvent));

            State.SetPrimitiveEvent(primitiveEvent);

            await ExecuteAsync().ConfigureAwait(false);
        }
    }
}