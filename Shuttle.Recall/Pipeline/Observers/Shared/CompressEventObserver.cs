using System;
using System.Threading.Tasks;
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
            ExecuteAsync(pipelineEvent, true).GetAwaiter().GetResult();
        }

        public async Task ExecuteAsync(OnCompressEvent pipelineEvent)
        {
            await ExecuteAsync(pipelineEvent, false).ConfigureAwait(false);
        }

        private async Task ExecuteAsync(OnCompressEvent pipelineEvent, bool sync)
        {
            var state = Guard.AgainstNull(pipelineEvent, nameof(pipelineEvent)).Pipeline.State;
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

            eventEnvelope.Event = sync
                ? algorithm.Compress(eventEnvelope.Event)
                : await algorithm.CompressAsync(eventEnvelope.Event).ConfigureAwait(false);
        }
    }
}