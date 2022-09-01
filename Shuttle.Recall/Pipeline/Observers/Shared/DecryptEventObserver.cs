using System;
using Shuttle.Core.Contract;
using Shuttle.Core.Encryption;
using Shuttle.Core.Pipelines;

namespace Shuttle.Recall
{
    public interface IDecryptEventObserver : IPipelineObserver<OnDecryptEvent>
    {
    }

    public class DecryptEventObserver : IDecryptEventObserver
    {
        private readonly IEncryptionService _encryptionService;

        public DecryptEventObserver(IEncryptionService encryptionService)
        {
            Guard.AgainstNull(encryptionService, nameof(encryptionService));

            _encryptionService = encryptionService;
        }

        public void Execute(OnDecryptEvent pipelineEvent)
        {
            var state = pipelineEvent.Pipeline.State;
            var eventEnvelope = state.GetEventEnvelope();

            if (!eventEnvelope.EncryptionEnabled())
            {
                return;
            }

            var algorithm = _encryptionService.Get(eventEnvelope.EncryptionAlgorithm);

            if (algorithm == null)
            {
                throw new InvalidOperationException(string.Format(
                    Resources.MissingCompressionAlgorithmException, eventEnvelope.CompressionAlgorithm));
            }

            eventEnvelope.Event = algorithm.Decrypt(eventEnvelope.Event);
        }
    }
}