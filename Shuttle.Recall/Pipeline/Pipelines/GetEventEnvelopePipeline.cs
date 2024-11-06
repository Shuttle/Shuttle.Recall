using System;
using System.Threading.Tasks;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Recall;

public class GetEventEnvelopePipeline : Pipeline
{
    public GetEventEnvelopePipeline(IServiceProvider serviceProvider, IDeserializeEventEnvelopeObserver deserializeEventEnvelopeObserver, IDecompressEventObserver decompressEventObserver, IDecryptEventObserver decryptEventObserver, IDeserializeEventObserver deserializeEventObserver) 
        : base(serviceProvider)
    {
        RegisterStage("GetEventEnvelope")
            .WithEvent<OnDeserializeEventEnvelope>()
            .WithEvent<OnAfterDeserializeEventEnvelope>()
            .WithEvent<OnDecompressEvent>()
            .WithEvent<OnAfterDecompressEvent>()
            .WithEvent<OnDecryptEvent>()
            .WithEvent<OnAfterDecryptEvent>()
            .WithEvent<OnDeserializeEvent>()
            .WithEvent<OnAfterDeserializeEvent>();

        RegisterObserver(Guard.AgainstNull(deserializeEventEnvelopeObserver));
        RegisterObserver(Guard.AgainstNull(decompressEventObserver));
        RegisterObserver(Guard.AgainstNull(decryptEventObserver));
        RegisterObserver(Guard.AgainstNull(deserializeEventObserver));
    }

    public async Task ExecuteAsync(PrimitiveEvent? primitiveEvent)
    {
        Guard.AgainstNull(primitiveEvent);

        State.SetPrimitiveEvent(primitiveEvent);

        await ExecuteAsync().ConfigureAwait(false);
    }
}