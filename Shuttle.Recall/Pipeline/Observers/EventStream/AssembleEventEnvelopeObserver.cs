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
        private readonly EventStoreOptions _options;

        public AssembleEventEnvelopeObserver(IOptions<EventStoreOptions> options)
        {
            Guard.AgainstNull(options, nameof(options));
            Guard.AgainstNull(options.Value, nameof(options.Value));

            _options = options.Value;
        }

        public void Execute(OnAssembleEventEnvelope pipelineEvent)
        {
            var state = pipelineEvent.Pipeline.State;
            var domainEvent = state.GetDomainEvent();
            var configurator = state.GetEventEnvelopeConfigurator();

            Guard.AgainstNull(domainEvent, nameof(domainEvent));

            var eventEnvelope = new EventEnvelope
            {
                Event = state.GetEventBytes(),
                EventType = domainEvent.Event.GetType().FullName,
                IsSnapshot = domainEvent.IsSnapshot,
                Version = domainEvent.Version,
                AssemblyQualifiedName = domainEvent.Event.GetType().AssemblyQualifiedName,
                EncryptionAlgorithm = _options.EncryptionAlgorithm,
                CompressionAlgorithm = _options.CompressionAlgorithm
            };

            if (configurator != null)
            {
                eventEnvelope.Headers = configurator.Headers;
            }

            state.SetEventEnvelope(eventEnvelope);
        }
    }
}