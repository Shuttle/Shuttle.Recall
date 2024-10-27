using System.Threading;

namespace Shuttle.Recall;

public interface IEventHandlerContext<out T> where T : class
{
    CancellationToken CancellationToken { get; }
    T Event { get; }
    EventEnvelope EventEnvelope { get; }
    PrimitiveEvent PrimitiveEvent { get; }
}