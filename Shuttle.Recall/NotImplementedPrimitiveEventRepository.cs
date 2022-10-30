using System;
using System.Collections.Generic;

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
    }
}