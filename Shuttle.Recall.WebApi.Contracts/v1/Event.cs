namespace Shuttle.Recall.WebApi.Contracts.v1;

public class Event
{
    public string DomainEvent { get; set; } = string.Empty;
    public EventEnvelope EventEnvelope { get; set; } = null!;
    public Query.PrimitiveEvent PrimitiveEvent { get; set; } = null!;
}
