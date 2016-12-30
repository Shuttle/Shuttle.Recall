using System;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Recall
{
	public class CompressEventObserver : IPipelineObserver<OnCompressEvent>
	{
	    private readonly IEventStoreConfiguration _configuration;

	    public CompressEventObserver(IEventStoreConfiguration configuration)
	    {
            Guard.AgainstNull(configuration, "configuration");

	        _configuration = configuration;
	    }

	    public void Execute(OnCompressEvent pipelineEvent)
		{
			var state = pipelineEvent.Pipeline.State;
			var eventEnvelope = state.GetEventEnvelope();

			if (!eventEnvelope.CompressionEnabled())
			{
				return;
			}

			var algorithm = _configuration.FindCompressionAlgorithm(eventEnvelope.CompressionAlgorithm);

	        if (algorithm == null)
	        {
	            throw new InvalidOperationException(
	                string.Format(InfrastructureResources.MissingCompressionAlgorithmException,
	                    eventEnvelope.CompressionAlgorithm));

	        }

			eventEnvelope.Event = algorithm.Compress(eventEnvelope.Event);
		}
	}
}