namespace Shuttle.Recall;

public class DomainEvent(object @event, int version)
{
    public object Event { get; } = @event;
    public int Version { get; } = version;
}