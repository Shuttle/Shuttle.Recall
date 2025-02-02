namespace Shuttle.Recall;

public class DomainEvent
{
    public DomainEvent(object @event, int version)
    {
        Event = @event;
        Version = version;
    }

    public object Event { get; }
    public int Version { get; }
}