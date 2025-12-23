using Shuttle.Core.Contract;
using Shuttle.Core.Encryption;
using Shuttle.Core.Pipelines;

namespace Shuttle.Recall;

public interface IDecryptEventObserver : IPipelineObserver<DecryptEvent>;

public class DecryptEventObserver(IEncryptionService encryptionService) : IDecryptEventObserver
{
    private readonly IEncryptionService _encryptionService = Guard.AgainstNull(encryptionService);

    public async Task ExecuteAsync(IPipelineContext<DecryptEvent> pipelineContext, CancellationToken cancellationToken = default)
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

        eventEnvelope.Event = await algorithm.DecryptAsync(eventEnvelope.Event, cancellationToken).ConfigureAwait(false);
    }
}