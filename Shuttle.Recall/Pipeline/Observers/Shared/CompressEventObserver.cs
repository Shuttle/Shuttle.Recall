using System;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Recall
{
    public interface ICompressEventObserver : IPipelineObserver<OnCompressEvent>
    {
    }

    public class CompressEventObserver : ICompressEventObserver
    {
        private readonly IEventStoreConfiguration _configuration;

        public CompressEventObserver(IEventStoreConfiguration configuration)
        {
            Guard.AgainstNull(configuration, nameof(configuration));

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
                    string.Format(Resources.MissingCompressionAlgorithmException,
                        eventEnvelope.CompressionAlgorithm));
            }

            eventEnvelope.Event = algorithm.Compress(eventEnvelope.Event);
        }
    }
}