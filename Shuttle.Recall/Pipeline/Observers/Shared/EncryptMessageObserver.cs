using System;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Recall.Shared
{
	public class EncryptMessageObserver : IPipelineObserver<OnEncryptEvent>
	{
        private readonly IEventStoreConfiguration _configuration;

        public EncryptMessageObserver(IEventStoreConfiguration configuration)
        {
            Guard.AgainstNull(configuration, "configuration");

            _configuration = configuration;
        }

        public void Execute(OnEncryptEvent pipelineEvent)
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

			transportMessage.Event = algorithm.Encrypt(transportMessage.Event);
		}
	}
}