using Shuttle.Core.Infrastructure;

namespace Shuttle.Recall
{
    public interface IEventHandlerContext<out T> where T : class
    {
        EventEnvelope EventEnvelope { get; }
        long SequenceNumber { get; }
        T Event { get; }
        IThreadState ActiveState { get; }
    }
}