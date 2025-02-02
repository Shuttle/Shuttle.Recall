using System;
using System.Threading.Tasks;
using Shuttle.Core.Contract;
using Shuttle.Core.Encryption;
using Shuttle.Core.Pipelines;

namespace Shuttle.Recall;

public interface IDecryptEventObserver : IPipelineObserver<OnDecryptEvent>
{
}

public class DecryptEventObserver : IDecryptEventObserver
{
    private readonly IEncryptionService _encryptionService;

    public DecryptEventObserver(IEncryptionService encryptionService)
    {
        _encryptionService = Guard.AgainstNull(encryptionService);
    }

    public async Task ExecuteAsync(IPipelineContext<OnDecryptEvent> pipelineContext)
    {
        var state = Guard.AgainstNull(pipelineContext).Pipeline.State;
        var eventEnvelope = state.GetEventEnvelope();

        if (!eventEnvelope.EncryptionEnabled())
        {
            return;
        }

        var algorithm = _encryptionService.Get(eventEnvelope.EncryptionAlgorithm);

        if (algorithm == null)
        {
            throw new InvalidOperationException(string.Format(Resources.MissingCompressionAlgorithmException, eventEnvelope.CompressionAlgorithm));
        }

        eventEnvelope.Event = await algorithm.DecryptAsync(eventEnvelope.Event).ConfigureAwait(false);
    }
}