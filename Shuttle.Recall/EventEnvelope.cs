using System;
using System.Collections.Generic;
using System.Linq;

namespace Shuttle.Recall;

[Serializable]
public class EventEnvelope
{
    public string AssemblyQualifiedName { get; set; } = string.Empty;
    public string CompressionAlgorithm { get; set; } = string.Empty;
    public string EncryptionAlgorithm { get; set; } = string.Empty;

    public byte[] Event { get; set; } = Array.Empty<byte>();
    public DateTime EventDate { get; set; } = DateTime.UtcNow;

    public Guid EventId { get; set; } = Guid.NewGuid();
    public string EventType { get; set; } = string.Empty;
    public List<EnvelopeHeader> Headers { get; set; } = new();
    public bool IsSnapshot { get; set; }
    public int Version { get; set; }
}