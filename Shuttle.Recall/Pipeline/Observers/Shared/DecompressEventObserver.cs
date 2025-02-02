using System;
using System.Threading.Tasks;
using Shuttle.Core.Compression;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Recall;

public interface IDecompressEventObserver : IPipelineObserver<OnDecompressEvent>
{
}

public class DecompressEventObserver : IDecompressEventObserver
{
    private readonly ICompressionService _compressionService;

    public DecompressEventObserver(ICompressionService compressionService)
    {
        _compressionService = Guard.AgainstNull(compressionService);
    }

    public async Task ExecuteAsync(IPipelineContext<OnDecompressEvent> pipelineContext)
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

        eventEnvelope.Event = await algorithm.DecompressAsync(eventEnvelope.Event);
    }
}