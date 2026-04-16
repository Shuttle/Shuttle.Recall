using Shuttle.Contract;

namespace Shuttle.Recall;

public class EventHandlerContext<T>(Projection projection, EventEnvelope eventEnvelope, T @event, PrimitiveEvent primitiveEvent)
    : IEventHandlerContext<T>
    where T : class
{
    public EventEnvelope EventEnvelope { get; } = Guard.AgainstNull(eventEnvelope);
    public PrimitiveEvent PrimitiveEvent { get; } = Guard.AgainstNull(primitiveEvent);
    public T Event { get; } = Guard.AgainstNull(@event);
    public Projection Projection { get; } = Guard.AgainstNull(projection);
    public TimeSpan? DeferredFor { get; private set; }
    public bool HasBeenDeferred { get; private set; }
    public void Defer(TimeSpan? delay = null)
    {
        DeferredFor = delay;
        HasBeenDeferred = true;
    }
}