namespace Shuttle.Recall;

[Serializable]
public class EventEnvelope
{
    public string AssemblyQualifiedName { get; set; } = string.Empty;
    public byte[] Event { get; set; } = [];
    public DateTimeOffset RecordedAt { get; set; } = DateTimeOffset.UtcNow;
    public Guid EventId { get; set; } = Guid.NewGuid();
    public string EventType { get; set; } = string.Empty;
    public List<EnvelopeHeader> Headers { get; set; } = [];
    public int Version { get; set; }
}