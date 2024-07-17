using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shuttle.Recall
{
    public class NotImplementedPrimitiveEventRepository : IPrimitiveEventRepository
    {
        public Task RemoveAsync(Guid id)
        {
            throw new NotImplementedException(Resources.NotImplementedPrimitiveEventRepository);
        }

        public Task<IEnumerable<PrimitiveEvent>> GetAsync(Guid id)
        {
            throw new NotImplementedException(Resources.NotImplementedPrimitiveEventRepository);
        }

        public ValueTask<long> SaveAsync(PrimitiveEvent primitiveEvent)
        {
            throw new NotImplementedException(Resources.NotImplementedPrimitiveEventRepository);
        }

        public ValueTask<long> GetSequenceNumberAsync(Guid id)
        {
            throw new NotImplementedException(Resources.NotImplementedPrimitiveEventRepository);
        }
    }
}