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
            var @event = state.GetEvent();
            var eventBytes = state.GetEventBytes();
            var configurator = state.GetEventEnvelopeConfigurator();

            Guard.AgainstNull(@event, "state.GetEvent()");
            Guard.AgainstNull(eventBytes, "state.GetEventBytes()");

            var eventEnvelope = new EventEnvelope
            {
                Event = eventBytes,
                AssemblyQualifiedName = @event.GetType().AssemblyQualifiedName,
                EventType = @event.GetType().FullName,
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