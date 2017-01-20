using Shuttle.Core.Infrastructure;

namespace Shuttle.Recall
{
    public interface IEventHandlerContext<out T> where T : class
    {
        EventEnvelope EventEnvelope { get; }
        PrimitiveEvent PrimitiveEvent { get; }
        T Event { get; }
        IThreadState ActiveState { get; }
    }
}