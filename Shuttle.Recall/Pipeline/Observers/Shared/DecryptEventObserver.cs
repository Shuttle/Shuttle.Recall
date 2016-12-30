using System;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Recall
{
    public class DecryptEventObserver : IPipelineObserver<OnDecryptEvent>
    {
        private readonly IEventStoreConfiguration _configuration;

        public DecryptEventObserver(IEventStoreConfiguration configuration)
        {
            Guard.AgainstNull(configuration, "configuration");

            _configuration = configuration;
        }

        public void Execute(OnDecryptEvent pipelineEvent)
        {
            var state = pipelineEvent.Pipeline.State;
            var eventEnvelope = state.GetEventEnvelope();

            if (!eventEnvelope.EncryptionEnabled())
            {
                return;
            }

            var algorithm = _configuration.FindEncryptionAlgorithm(eventEnvelope.EncryptionAlgorithm);

            if (algorithm == null)
            {
                throw new InvalidOperationException(string.Format(InfrastructureResources.MissingCompressionAlgorithmException, eventEnvelope.CompressionAlgorithm));
            }

            eventEnvelope.Event = algorithm.Decrypt(eventEnvelope.Event);
        }
    }
}