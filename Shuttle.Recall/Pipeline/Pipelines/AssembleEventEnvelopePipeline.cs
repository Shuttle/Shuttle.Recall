using System.Threading.Tasks;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Recall;

public class AssembleEventEnvelopePipeline : Pipeline
{
    public AssembleEventEnvelopePipeline(IAssembleEventEnvelopeObserver assembleEventEnvelopeObserver,
        ICompressEventObserver compressEventObserver, IEncryptEventObserver encryptEventObserver,
        ISerializeEventObserver serializeEventObserver)
    {
        RegisterStage("Get")
            .WithEvent<OnAssembleEventEnvelope>()
            .WithEvent<OnAfterAssembleEventEnvelope>()
            .WithEvent<OnSerializeEvent>()
            .WithEvent<OnAfterSerializeEvent>()
            .WithEvent<OnEncryptEvent>()
            .WithEvent<OnAfterEncryptEvent>()
            .WithEvent<OnCompressEvent>()
            .WithEvent<OnAfterCompressEvent>();

        RegisterObserver(Guard.AgainstNull(assembleEventEnvelopeObserver));
        RegisterObserver(Guard.AgainstNull(compressEventObserver));
        RegisterObserver(Guard.AgainstNull(encryptEventObserver));
        RegisterObserver(Guard.AgainstNull(serializeEventObserver));
    }

    public async Task<EventEnvelope> ExecuteAsync(DomainEvent domainEvent)
    {
        State.SetDomainEvent(Guard.AgainstNull(domainEvent));

        await ExecuteAsync().ConfigureAwait(false);

        return Guard.AgainstNull(State.GetEventEnvelope());
    }
}