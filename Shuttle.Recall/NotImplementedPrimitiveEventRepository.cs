using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shuttle.Recall
{
    public class NotImplementedPrimitiveEventRepository : IPrimitiveEventRepository
    {
        public void Remove(Guid id)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<PrimitiveEvent> Get(Guid id)
        {
            throw new NotImplementedException();
        }

        public long Save(PrimitiveEvent primitiveEvent)
        {
            throw new NotImplementedException();
        }

        public long GetSequenceNumber(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task RemoveAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<PrimitiveEvent>> GetAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public ValueTask<long> SaveAsync(PrimitiveEvent primitiveEvent)
        {
            throw new NotImplementedException();
        }

        public Task<long> GetSequenceNumberAsync(Guid id)
        {
            throw new NotImplementedException();
        }
    }
}