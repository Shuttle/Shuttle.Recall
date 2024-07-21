using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shuttle.Recall
{
    public interface IPrimitiveEventRepository
    {
        void Remove(Guid id);
        IEnumerable<PrimitiveEvent> Get(Guid id);
        long Save(PrimitiveEvent primitiveEvent);
        long GetSequenceNumber(Guid id);

        Task RemoveAsync(Guid id);
        Task<IEnumerable<PrimitiveEvent>> GetAsync(Guid id);
        ValueTask<long> SaveAsync(PrimitiveEvent primitiveEvent);
        ValueTask<long> GetSequenceNumberAsync(Guid id);
    }
}