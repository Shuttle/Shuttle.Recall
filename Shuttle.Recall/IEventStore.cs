using System;
using System.Threading.Tasks;

namespace Shuttle.Recall
{
    public interface IEventStore
    {
        EventStream Get(Guid id);
        Task<EventStream> GetAsync(Guid id);
        long Save(EventStream eventStream, Action<EventEnvelopeBuilder> builder = null);
        ValueTask<long> SaveAsync(EventStream eventStream, Action<EventEnvelopeBuilder> builder = null);
        void Remove(Guid id);
        Task RemoveAsync(Guid id);
    }
}