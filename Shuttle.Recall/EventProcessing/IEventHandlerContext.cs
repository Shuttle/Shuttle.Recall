namespace Shuttle.Recall;

public interface IEventHandlerContext<out T> where T : class
{
    T Event { get; }
    EventEnvelope EventEnvelope { get; }
    PrimitiveEvent PrimitiveEvent { get; }
    public Projection Projection { get; }
}