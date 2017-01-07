using Shuttle.Core.Infrastructure;

namespace Shuttle.Recall
{
    public class AssembleEventEnvelopeObserver : IPipelineObserver<OnAssembleEventEnvelope>
    {
        private readonly IEventStoreConfiguration _configuration;

        public AssembleEventEnvelopeObserver(IEventStoreConfiguration configuration)
        {
            Guard.AgainstNull(configuration, "configuration");

            _configuration = configuration;
        }

        public void Execute(OnAssembleEventEnvelope pipelineEvent)
        {
            var state = pipelineEvent.Pipeline.State;
            var domainEvent = state.GetDomainEvent();
            var configurator = state.GetEventEnvelopeConfigurator();

            Guard.AgainstNull(domainEvent, "state.GetDomainEvent()");

            var eventEnvelope = new EventEnvelope
            {
                AssemblyQualifiedName = domainEvent.GetType().AssemblyQualifiedName,
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