using System;

namespace Shuttle.Recall
{
    public class PrimitiveEvent
    {
        public Guid Id { get; set; }
        public int Version { get; set; }
        public bool IsSnapshot { get; set; }
        public Guid EventId { get; set; }
        public string EventType { get; set; }
        public byte[] EventEnvelope { get; set; }
        public long SequenceNumber { get; set; }
        public DateTime DateRegistered { get; set; }
    }
}