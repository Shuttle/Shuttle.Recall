using System;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Recall.Shared
{
	public class CompressMessageObserver : IPipelineObserver<OnCompressEvent>
	{
	    private readonly IEventStoreConfiguration _configuration;

	    public CompressMessageObserver(IEventStoreConfiguration configuration)
	    {
            Guard.AgainstNull(configuration, "configuration");

	        _configuration = configuration;
	    }

	    public void Execute(OnCompressEvent pipelineEvent)
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
	            throw new InvalidOperationException(
	                string.Format(InfrastructureResources.MissingCompressionAlgorithmException,
	                    transportMessage.CompressionAlgorithm));

	        }

			transportMessage.Event = algorithm.Compress(transportMessage.Event);
		}
	}
}