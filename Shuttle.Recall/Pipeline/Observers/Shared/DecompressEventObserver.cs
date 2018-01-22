using System;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Recall
{
    public interface IDecompressEventObserver : IPipelineObserver<OnDecompressEvent>
    {
    }

    public class DecompressEventObserver : IDecompressEventObserver
    {
        private readonly IEventStoreConfiguration _configuration;

        public DecompressEventObserver(IEventStoreConfiguration configuration)
        {
            Guard.AgainstNull(configuration, nameof(configuration));

            _configuration = configuration;
        }

        public void Execute(OnDecompressEvent pipelineEvent)
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
                throw new InvalidOperationException(string.Format(
                    Resources.MissingCompressionAlgorithmException, eventEnvelope.CompressionAlgorithm));
            }

            eventEnvelope.Event = algorithm.Decompress(eventEnvelope.Event);
        }
    }
}