using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.TransactionScope;

namespace Shuttle.Recall;

public interface IGetEventEnvelopePipeline : IPipeline
{
    Task ExecuteAsync(PrimitiveEvent? primitiveEvent);
}

public class GetEventEnvelopePipeline : Pipeline, IGetEventEnvelopePipeline
{
    public GetEventEnvelopePipeline(IOptions<PipelineOptions> pipelineOptions, IOptions<TransactionScopeOptions> transactionScopeOptions, ITransactionScopeFactory transactionScopeFactory, IServiceProvider serviceProvider, IDeserializeEventEnvelopeObserver deserializeEventEnvelopeObserver, IDecompressEventObserver decompressEventObserver, IDecryptEventObserver decryptEventObserver, IDeserializeEventObserver deserializeEventObserver)
        : base(pipelineOptions, transactionScopeOptions, transactionScopeFactory, serviceProvider)
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