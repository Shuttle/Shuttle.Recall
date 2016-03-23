using Shuttle.Core.Infrastructure;

namespace Shuttle.Recall
{
    public interface IEventHandlerContext<out T> where T : class
    {
        ProjectionEvent ProjectionEvent { get; }
        T DomainEvent { get; }
        IThreadState ActiveState { get; }
    }
}