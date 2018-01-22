using System;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Recall
{
    public interface IEncryptEventObserver : IPipelineObserver<OnEncryptEvent>
    {
    }

    public class EncryptEventObserver : IEncryptEventObserver
    {
        private readonly IEventStoreConfiguration _configuration;

        public EncryptEventObserver(IEventStoreConfiguration configuration)
        {
            Guard.AgainstNull(configuration, nameof(configuration));

            _configuration = configuration;
        }

        public void Execute(OnEncryptEvent pipelineEvent)
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

            eventEnvelope.Event = algorithm.Encrypt(eventEnvelope.Event);
        }
    }
}