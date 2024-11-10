using System;
using System.Threading.Tasks;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Recall;

public class AssembleEventEnvelopePipeline : Pipeline
{
    public AssembleEventEnvelopePipeline(IServiceProvider serviceProvider, IAssembleEventEnvelopeObserver assembleEventEnvelopeObserver, ICompressEventObserver compressEventObserver, IEncryptEventObserver encryptEventObserver, ISerializeEventObserver serializeEventObserver) 
        : base(serviceProvider)
    {
        AddStage("Get")
            .WithEvent<OnSerializeEvent>()
            .WithEvent<OnAfterSerializeEvent>()
            .WithEvent<OnAssembleEventEnvelope>()
            .WithEvent<OnAfterAssembleEventEnvelope>()
            .WithEvent<OnEncryptEvent>()
            .WithEvent<OnAfterEncryptEvent>()
            .WithEvent<OnCompressEvent>()
            .WithEvent<OnAfterCompressEvent>();

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