using System.Threading;
using Shuttle.Core.Contract;

namespace Shuttle.Recall;

public class EventHandlerContext<T> : IEventHandlerContext<T> where T : class
{
    public EventHandlerContext(EventEnvelope eventEnvelope, T @event, PrimitiveEvent primitiveEvent, CancellationToken cancellationToken)
    {
        EventEnvelope = Guard.AgainstNull(eventEnvelope);
        Event = Guard.AgainstNull(@event);
        PrimitiveEvent = Guard.AgainstNull(primitiveEvent);
        CancellationToken = cancellationToken;
    }

    public EventEnvelope EventEnvelope { get; }
    public PrimitiveEvent PrimitiveEvent { get; }
    public T Event { get; }
    public CancellationToken CancellationToken { get; }
}