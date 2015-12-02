using Shuttle.Core.Infrastructure;

namespace Shuttle.Recall.Core
{
    public interface IEventHandlerContext<out T> where T : class
    {
        EventRead EventRead { get; }
        T DomainEvent { get; }
        IThreadState ActiveState { get; }
    }
}