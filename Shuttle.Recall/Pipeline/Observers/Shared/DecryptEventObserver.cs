using System;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Recall
{
    public interface IDecryptEventObserver : IPipelineObserver<OnDecryptEvent>
    {
    }

    public class DecryptEventObserver : IDecryptEventObserver
    {
        private readonly IEventStoreConfiguration _configuration;

        public DecryptEventObserver(IEventStoreConfiguration configuration)
        {
            Guard.AgainstNull(configuration, nameof(configuration));

            _configuration = configuration;
        }

        public void Execute(OnDecryptEvent pipelineEvent)
        {
            var state = pipelineEvent.Pipeline.State;
            var eventEnvelope = state.GetEventEnvelope();

            if (!eventEnvelope.EncryptionEnabled())
            {
                return;
            }

            var algorithm = _configuration.FindEncryptionAlgorithm(eventEnvelope.EncryptionAlgorithm);

            if (algorithm == null)
            {
                throw new InvalidOperationException(string.Format(
                    Resources.MissingCompressionAlgorithmException, eventEnvelope.CompressionAlgorithm));
            }

            eventEnvelope.Event = algorithm.Decrypt(eventEnvelope.Event);
        }
    }
}