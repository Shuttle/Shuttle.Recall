using System;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Recall.Shared
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
            var transportMessage = state.GetEventEnvelope();

            if (!transportMessage.EncryptionEnabled())
            {
                return;
            }

            var algorithm = _configuration.FindEncryptionAlgorithm(transportMessage.EncryptionAlgorithm);

            if (algorithm == null)
            {
                throw new InvalidOperationException(string.Format(InfrastructureResources.MissingCompressionAlgorithmException, transportMessage.CompressionAlgorithm));
            }

            transportMessage.Event = algorithm.Decrypt(transportMessage.Event);
        }
    }
}