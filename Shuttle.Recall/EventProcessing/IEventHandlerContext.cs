namespace Shuttle.Recall;

public interface IEventHandlerContext
{
    EventEnvelope EventEnvelope { get; }
    PrimitiveEvent PrimitiveEvent { get; }
    Projection Projection { get; }

    bool HasBeenDeferred { get; }

    /// <summary>
    /// The amount of time to wait before retrying the event.
    /// </summary>
    TimeSpan? DeferredFor { get; }

    /// <summary>
    /// Marks the event to be processed again after the specified delay.
    /// </summary>
    void Defer(TimeSpan? delay = null);
}

public interface IEventHandlerContext<out T> : IEventHandlerContext where T : class
{
    T Event { get; }
}