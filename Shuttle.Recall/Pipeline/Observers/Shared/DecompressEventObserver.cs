using System;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Recall.Shared
{
	public class DecompressEventObserver : IPipelineObserver<OnDecompressEvent>
	{
        private readonly IEventStoreConfiguration _configuration;

        public DecompressEventObserver(IEventStoreConfiguration configuration)
        {
            Guard.AgainstNull(configuration, "configuration");

            _configuration = configuration;
        }

        public void Execute(OnDecompressEvent pipelineEvent)
		{
			var state = pipelineEvent.Pipeline.State;
			var transportMessage = state.GetEventEnvelope();

			if (!transportMessage.CompressionEnabled())
			{
				return;
			}

			var algorithm = _configuration.FindCompressionAlgorithm(transportMessage.CompressionAlgorithm);

            if (algorithm == null)
            {
                throw new InvalidOperationException(string.Format(InfrastructureResources.MissingCompressionAlgorithmException, transportMessage.CompressionAlgorithm));
            }

			transportMessage.Event = algorithm.Decompress(transportMessage.Event);
		}
	}
}