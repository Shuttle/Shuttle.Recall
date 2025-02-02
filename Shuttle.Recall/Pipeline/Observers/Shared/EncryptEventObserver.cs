using System;
using System.Threading.Tasks;
using Shuttle.Core.Contract;
using Shuttle.Core.Encryption;
using Shuttle.Core.Pipelines;

namespace Shuttle.Recall;

public interface IEncryptEventObserver : IPipelineObserver<OnEncryptEvent>
{
}

public class EncryptEventObserver : IEncryptEventObserver
{
    private readonly IEncryptionService _encryptionService;

    public EncryptEventObserver(IEncryptionService encryptionService)
    {
        _encryptionService = Guard.AgainstNull(encryptionService);
    }

    public async Task ExecuteAsync(IPipelineContext<OnEncryptEvent> pipelineContext)
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

        eventEnvelope.Event = await algorithm.EncryptAsync(eventEnvelope.Event).ConfigureAwait(false);
    }
}