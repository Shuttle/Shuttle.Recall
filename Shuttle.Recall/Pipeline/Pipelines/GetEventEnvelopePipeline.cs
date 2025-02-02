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
        AddStage("GetEventEnvelope")
            .WithEvent<OnDeserializeEventEnvelope>()
            .WithEvent<OnAfterDeserializeEventEnvelope>()
            .WithEvent<OnDecompressEvent>()
            .WithEvent<OnAfterDecompressEvent>()
            .WithEvent<OnDecryptEvent>()
            .WithEvent<OnAfterDecryptEvent>()
            .WithEvent<OnDeserializeEvent>()
            .WithEvent<OnAfterDeserializeEvent>();

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