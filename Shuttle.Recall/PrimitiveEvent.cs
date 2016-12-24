using System;

namespace Shuttle.Recall
{
    public class PrimitiveEvent
    {
        public Guid Id { get; set; }
        public int Version { get; set; }
        public byte[] TypeHash { get; set; }
        public byte[] EventEnvelope { get; set; }
        public long SequenceNumber { get; set; }
        public DateTime DateRegistered { get; set; } 
    }
}