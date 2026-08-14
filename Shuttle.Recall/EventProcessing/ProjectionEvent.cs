using Shuttle.Contract;

namespace Shuttle.Recall;

public class ProjectionEvent(Projection projection, PrimitiveEvent primitiveEvent, bool alreadyHandled = false)
{
    public PrimitiveEvent PrimitiveEvent { get; } = Guard.AgainstNull(primitiveEvent);

    public Projection Projection { get; } = Guard.AgainstNull(projection);

    /// <summary>
    /// Set by the event-processing implementation when this event has already been handled by this projection
    /// via immediate consistency. The handler is skipped, but the projection's checkpoint still advances across it.
    /// </summary>
    public bool AlreadyHandled { get; } = alreadyHandled;
}