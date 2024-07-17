using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Recall
{
    public interface IAssembleEventEnvelopeObserver : IPipelineObserver<OnAssembleEventEnvelope>
    {
    }

    public class AssembleEventEnvelopeObserver : IAssembleEventEnvelopeObserver
    {
        private readonly EventStoreOptions _eventStoreOptions;

        public AssembleEventEnvelopeObserver(IOptions<EventStoreOptions> eventStoreOptions)
        {
            Guard.AgainstNull(eventStoreOptions, nameof(eventStoreOptions));

            _eventStoreOptions = Guard.AgainstNull(eventStoreOptions.Value, nameof(eventStoreOptions.Value));
        }

        public void Execute(OnAssembleEventEnvelope pipelineEvent)
        {
            ExecuteAsync(pipelineEvent, true).GetAwaiter().GetResult();
        }

        public async Task ExecuteAsync(OnAssembleEventEnvelope pipelineEvent)
        {
            await ExecuteAsync(pipelineEvent, false).ConfigureAwait(false);
        }

        private async Task ExecuteAsync(OnAssembleEventEnvelope pipelineEvent, bool sync)
        {
            var state = Guard.AgainstNull(pipelineEvent, nameof(pipelineEvent)).Pipeline.State;
            var domainEvent = Guard.AgainstNull(state.GetDomainEvent(), StateKeys.DomainEvent);
            var builder = state.GetEventStreamBuilder();

            var eventEnvelope = new EventEnvelope
            {
                Event = state.GetEventBytes(),
                EventType = domainEvent.Event.GetType().FullName,
                IsSnapshot = domainEvent.IsSnapshot,
                Version = domainEvent.Version,
                AssemblyQualifiedName = domainEvent.Event.GetType().AssemblyQualifiedName,
                EncryptionAlgorithm = _eventStoreOptions.EncryptionAlgorithm,
                CompressionAlgorithm = _eventStoreOptions.CompressionAlgorithm
            };

            if (builder != null)
            {
                eventEnvelope.Headers = builder.Headers;
            }

            state.SetEventEnvelope(eventEnvelope);

            await Task.CompletedTask;
        }
    }
}