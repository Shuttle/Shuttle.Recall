using Shuttle.Core.Contract;

namespace Shuttle.Recall;

public class ProjectionEvent(Projection projection, PrimitiveEvent primitiveEvent)
{
    public PrimitiveEvent PrimitiveEvent { get; } = Guard.AgainstNull(primitiveEvent);

    public Projection Projection { get; } = Guard.AgainstNull(projection);
}