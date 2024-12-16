using Shuttle.Core.Contract;

namespace Shuttle.Recall;

public class ProjectionEvent
{
    public ProjectionEvent(Projection projection, PrimitiveEvent primitiveEvent)
    {
        Projection = Guard.AgainstNull(projection);
        PrimitiveEvent = Guard.AgainstNull(primitiveEvent);
    }

    public Projection Projection { get; }
    public PrimitiveEvent PrimitiveEvent { get; }
}