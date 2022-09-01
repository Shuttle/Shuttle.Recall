using System;
using Shuttle.Core.Compression;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Recall
{
    public interface ICompressEventObserver : IPipelineObserver<OnCompressEvent>
    {
    }

    public class CompressEventObserver : ICompressEventObserver
    {
        private readonly ICompressionService _compressionService;

        public CompressEventObserver(ICompressionService compressionService)
        {
            Guard.AgainstNull(compressionService, nameof(compressionService));

            _compressionService = compressionService;
        }

        public void Execute(OnCompressEvent pipelineEvent)
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
                throw new InvalidOperationException(
                    string.Format(Resources.MissingCompressionAlgorithmException,
                        eventEnvelope.CompressionAlgorithm));
            }

            eventEnvelope.Event = algorithm.Compress(eventEnvelope.Event);
        }
    }
}