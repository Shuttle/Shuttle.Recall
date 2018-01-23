using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Recall
{
    public class AssembleEventEnvelopePipeline : Pipeline
    {
        public AssembleEventEnvelopePipeline(IAssembleEventEnvelopeObserver assembleEventEnvelopeObserver,
            ICompressEventObserver compressEventObserver, IEncryptEventObserver encryptEventObserver,
            ISerializeEventObserver serializeEventObserver)
        {
            Guard.AgainstNull(assembleEventEnvelopeObserver, nameof(assembleEventEnvelopeObserver));
            Guard.AgainstNull(compressEventObserver, nameof(compressEventObserver));
            Guard.AgainstNull(encryptEventObserver, nameof(encryptEventObserver));
            Guard.AgainstNull(serializeEventObserver, nameof(serializeEventObserver));

            RegisterStage("Get")
                .WithEvent<OnAssembleEventEnvelope>()
                .WithEvent<OnAfterAssembleEventEnvelope>()
                .WithEvent<OnSerializeEvent>()
                .WithEvent<OnAfterSerializeEvent>()
                .WithEvent<OnEncryptEvent>()
                .WithEvent<OnAfterEncryptEvent>()
                .WithEvent<OnCompressEvent>()
                .WithEvent<OnAfterCompressEvent>()
                .WithEvent<OnAssembleEventEnvelope>()
                .WithEvent<OnAfterAssembleEventEnvelope>();

            RegisterObserver(assembleEventEnvelopeObserver);
            RegisterObserver(compressEventObserver);
            RegisterObserver(encryptEventObserver);
            RegisterObserver(serializeEventObserver);
        }

        public EventEnvelope Execute(DomainEvent domainEvent)
        {
            State.SetDomainEvent(domainEvent);

            Execute();

            return State.GetEventEnvelope();
        }
    }
}