using Shuttle.Core.Compression;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Recall;

public interface ICompressEventObserver : IPipelineObserver<CompressEvent>;

public class CompressEventObserver(ICompressionService compressionService) : ICompressEventObserver
{
    private readonly ICompressionService _compressionService = Guard.AgainstNull(compressionService);


    public async Task ExecuteAsync(IPipelineContext<CompressEvent> pipelineContext, CancellationToken cancellationToken = default)
    {
        var state = Guard.AgainstNull(pipelineContext).Pipeline.State;
        var eventEnvelope = state.GetEventEnvelope();

        if (!eventEnvelope.CompressionEnabled())
        {
            return;
        }

        var algorithm = _compressionService.Get(eventEnvelope.CompressionAlgorithm);

        if (algorithm == null)
        {
            throw new InvalidOperationException(string.Format(Resources.MissingCompressionAlgorithmException, eventEnvelope.CompressionAlgorithm));
        }

        eventEnvelope.Event = await algorithm.CompressAsync(eventEnvelope.Event, cancellationToken).ConfigureAwait(false);
    }
}