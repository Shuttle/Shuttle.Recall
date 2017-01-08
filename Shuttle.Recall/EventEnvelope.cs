using System;
using System.Collections.Generic;

namespace Shuttle.Recall
{
    [Serializable]
    public class EventEnvelope
    {
        public EventEnvelope()
        {
            EventId = Guid.NewGuid();
            EventDate = DateTime.Now;
            Headers = new List<EnvelopeHeader>();
        }

        public byte[] Event { get; set; }

        public Guid EventId { get; set; }
        public DateTime EventDate { get; set; }
        public string AssemblyQualifiedName { get; set; }
        public string EncryptionAlgorithm { get; set; }
        public string CompressionAlgorithm { get; set; }
        public List<EnvelopeHeader> Headers { get; set; }
        public string EventType { get; set; }
        public bool IsSnapshot { get; set; }
        public int Version { get; set; }
    }
}