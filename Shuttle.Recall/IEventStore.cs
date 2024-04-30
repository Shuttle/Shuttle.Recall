using System;
using System.Threading.Tasks;

namespace Shuttle.Recall
{
    public interface IEventStore
    {
        EventStream Get(Guid id, Action<EventStreamBuilder> builder = null);
        Task<EventStream> GetAsync(Guid id, Action<EventStreamBuilder> builder = null);
        long Save(EventStream eventStream, Action<SaveEventStreamBuilder> builder = null);
        ValueTask<long> SaveAsync(EventStream eventStream, Action<SaveEventStreamBuilder> builder = null);
        void Remove(Guid id, Action<EventStreamBuilder> builder = null);
        Task RemoveAsync(Guid id, Action<EventStreamBuilder> builder = null);
    }
}