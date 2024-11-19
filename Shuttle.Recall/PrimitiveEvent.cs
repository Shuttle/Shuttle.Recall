using System;

namespace Shuttle.Recall;

public class PrimitiveEvent
{
    public DateTime DateRegistered { get; set; }
    public byte[] EventEnvelope { get; set; } = Array.Empty<byte>();
    public Guid EventId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public Guid Id { get; set; }
    public Guid? CorrelationId { get; set; }
    public long SequenceNumber { get; set; }
    public int Version { get; set; }
}