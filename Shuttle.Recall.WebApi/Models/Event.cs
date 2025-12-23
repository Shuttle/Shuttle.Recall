namespace Shuttle.Recall.WebApi.Models;

public class Event
{
    public string DomainEvent { get; set; } = string.Empty;
    public EventEnvelope EventEnvelope { get; set; } = null!;
    public Recall.PrimitiveEvent PrimitiveEvent { get; set; } = null!;
}