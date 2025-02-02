using System.Threading;
using Shuttle.Core.Contract;

namespace Shuttle.Recall;

public class EventHandlerContext<T> : IEventHandlerContext<T> where T : class
{
    public EventHandlerContext(Projection projection, EventEnvelope eventEnvelope, T @event, PrimitiveEvent primitiveEvent, CancellationToken cancellationToken)
    {
        Projection = Guard.AgainstNull(projection);
        EventEnvelope = Guard.AgainstNull(eventEnvelope);
        Event = Guard.AgainstNull(@event);
        PrimitiveEvent = Guard.AgainstNull(primitiveEvent);
        CancellationToken = cancellationToken;
    }

    public EventEnvelope EventEnvelope { get; }
    public PrimitiveEvent PrimitiveEvent { get; }
    public T Event { get; }
    public Projection Projection { get; }
    public CancellationToken CancellationToken { get; }
}