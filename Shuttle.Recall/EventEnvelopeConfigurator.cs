using System;
using System.Collections.Generic;
using Shuttle.Core.Contract;

namespace Shuttle.Recall
{
    public class EventEnvelopeConfigurator
    {
        public EventEnvelopeConfigurator()
        {
            Headers = new List<EnvelopeHeader>();
        }

        public List<EnvelopeHeader> Headers { get; set; }

        public EventEnvelope EventEnvelope(object @event, string encryptionAlgorithm, string compressionAlgorithm)
        {
            Guard.AgainstNull(@event, nameof(@event));

            var result = new EventEnvelope
            {
                AssemblyQualifiedName = @event.GetType().AssemblyQualifiedName,
                EncryptionAlgorithm = encryptionAlgorithm,
                CompressionAlgorithm = compressionAlgorithm,
                EventDate = DateTime.Now
            };

            result.Headers.Merge(Headers);

            return result;
        }
    }
}