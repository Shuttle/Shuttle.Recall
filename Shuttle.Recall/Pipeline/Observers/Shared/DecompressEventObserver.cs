using System;
using Shuttle.Core.Compression;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Recall
{
    public interface IDecompressEventObserver : IPipelineObserver<OnDecompressEvent>
    {
    }

    public class DecompressEventObserver : IDecompressEventObserver
    {
        private readonly ICompressionService _compressionService;

        public DecompressEventObserver(ICompressionService compressionService)
        {
            Guard.AgainstNull(compressionService, nameof(compressionService));

            _compressionService = compressionService;
        }

        public void Execute(OnDecompressEvent pipelineEvent)
        {
            var state = pipelineEvent.Pipeline.State;
            var eventEnvelope = state.GetEventEnvelope();

            if (!eventEnvelope.CompressionEnabled())
            {
                return;
            }

            var algorithm = _compressionService.Get(eventEnvelope.CompressionAlgorithm);

            if (algorithm == null)
            {
                throw new InvalidOperationException(string.Format(
                    Resources.MissingCompressionAlgorithmException, eventEnvelope.CompressionAlgorithm));
            }

            eventEnvelope.Event = algorithm.Decompress(eventEnvelope.Event);
        }
    }
}