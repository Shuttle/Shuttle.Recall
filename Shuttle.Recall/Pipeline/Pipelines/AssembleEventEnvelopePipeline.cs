using System.Threading.Tasks;
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
            RegisterStage("Get")
                .WithEvent<OnAssembleEventEnvelope>()
                .WithEvent<OnAfterAssembleEventEnvelope>()
                .WithEvent<OnSerializeEvent>()
                .WithEvent<OnAfterSerializeEvent>()
                .WithEvent<OnEncryptEvent>()
                .WithEvent<OnAfterEncryptEvent>()
                .WithEvent<OnCompressEvent>()
                .WithEvent<OnAfterCompressEvent>();

            RegisterObserver(Guard.AgainstNull(assembleEventEnvelopeObserver, nameof(assembleEventEnvelopeObserver)));
            RegisterObserver(Guard.AgainstNull(compressEventObserver, nameof(compressEventObserver)));
            RegisterObserver(Guard.AgainstNull(encryptEventObserver, nameof(encryptEventObserver)));
            RegisterObserver(Guard.AgainstNull(serializeEventObserver, nameof(serializeEventObserver)));
        }

        public EventEnvelope Execute(DomainEvent domainEvent)
        {
            return ExecuteAsync(domainEvent, true).GetAwaiter().GetResult();
        }

        public async Task<EventEnvelope> ExecuteAsync(DomainEvent domainEvent)
        {
            return await ExecuteAsync(domainEvent, false).ConfigureAwait(false);
        }

        private async Task<EventEnvelope> ExecuteAsync(DomainEvent domainEvent, bool sync)
        {
            State.SetDomainEvent(Guard.AgainstNull(domainEvent, nameof(domainEvent)));

            if (sync)
            {
                Execute();
            }
            else
            {
                await ExecuteAsync().ConfigureAwait(false);
            }

            return State.GetEventEnvelope();
        }
    }
}