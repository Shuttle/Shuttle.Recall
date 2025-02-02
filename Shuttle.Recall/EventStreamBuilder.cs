using System.Collections.Generic;

namespace Shuttle.Recall;

public class EventStreamBuilder
{
    public List<EnvelopeHeader> Headers { get; set; } = new();
}