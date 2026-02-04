using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.TransactionScope;

namespace Shuttle.Recall;

public class AssembleEventEnvelopePipeline : Pipeline
{
    public AssembleEventEnvelopePipeline(IOptions<PipelineOptions> pipelineOptions, IOptions<TransactionScopeOptions> transactionScopeOptions, ITransactionScopeFactory transactionScopeFactory, IServiceProvider serviceProvider, IAssembleEventEnvelopeObserver assembleEventEnvelopeObserver, ICompressEventObserver compressEventObserver, IEncryptEventObserver encryptEventObserver, ISerializeEventObserver serializeEventObserver)
        : base(pipelineOptions, transactionScopeOptions, transactionScopeFactory, serviceProvider)
    {
        AddStage("Get")
            .WithEvent<SerializeEvent>()
            .WithEvent<EventSerialized>()
            .WithEvent<AssembleEventEnvelope>()
            .WithEvent<EventEnvelopeAssembled>()
            .WithEvent<EncryptEvent>()
            .WithEvent<EventEncrypted>()
            .WithEvent<CompressEvent>()
            .WithEvent<EventCompressed>();

        AddObserver(Guard.AgainstNull(serializeEventObserver));
        AddObserver(Guard.AgainstNull(encryptEventObserver));
        AddObserver(Guard.AgainstNull(compressEventObserver));
        AddObserver(Guard.AgainstNull(assembleEventEnvelopeObserver));
    }

    public async Task<EventEnvelope> ExecuteAsync(DomainEvent domainEvent)
    {
        State.SetDomainEvent(Guard.AgainstNull(domainEvent));

        await ExecuteAsync().ConfigureAwait(false);

        return State.GetEventEnvelope();
    }
}