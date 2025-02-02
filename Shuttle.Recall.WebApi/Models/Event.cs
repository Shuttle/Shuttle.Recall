namespace Shuttle.Recall.WebApi.Models;

public class Event
{
    public Recall.PrimitiveEvent PrimitiveEvent { get; set; } = null!;
    public EventEnvelope EventEnvelope { get; set; } = null!;
    public string DomainEvent { get; set; } = string.Empty;
}