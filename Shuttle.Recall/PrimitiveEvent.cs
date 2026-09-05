using Shuttle.Contract;

namespace Shuttle.Recall;

public class PrimitiveEvent(Guid id, Guid eventId, int version, string eventType, byte[] eventEnvelope, DateTimeOffset recordedAt, Guid? correlationId = null, long? sequenceNumber = null)
{
    public Guid? CorrelationId { get; private set; } = correlationId;
    public DateTimeOffset RecordedAt { get; } = recordedAt;
    public byte[] EventEnvelope { get; } = eventEnvelope;
    public Guid EventId { get; } = eventId;
    public string EventType { get; } = eventType;
    public Guid Id { get; } = id;
    public long? SequenceNumber { get; private set; } = sequenceNumber;
    public int Version { get; } = version;

    public PrimitiveEvent WithCorrelationId(Guid correlationId)
    {
        CorrelationId = correlationId;
        return this;
    }

    public PrimitiveEvent WithSequenceNumber(long sequenceNumber)
    {
        SequenceNumber = sequenceNumber;
        return this;
    }
}