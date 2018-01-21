using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Recall
{
    public class AssembleEventEnvelopeObserver : IPipelineObserver<OnAssembleEventEnvelope>
    {
        private readonly IEventStoreConfiguration _configuration;

        public AssembleEventEnvelopeObserver(IEventStoreConfiguration configuration)
        {
            Guard.AgainstNull(configuration, nameof(configuration));

            _configuration = configuration;
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
                EncryptionAlgorithm = _configuration.EncryptionAlgorithm,
                CompressionAlgorithm = _configuration.CompressionAlgorithm
            };

            if (configurator != null)
            {
                eventEnvelope.Headers = configurator.Headers;
            }

            state.SetEventEnvelope(eventEnvelope);
        }
    }
}