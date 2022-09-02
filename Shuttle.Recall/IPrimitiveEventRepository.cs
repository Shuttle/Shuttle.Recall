using System;
using System.Collections.Generic;

namespace Shuttle.Recall
{
    public interface IPrimitiveEventRepository
    {
        void Remove(Guid id);
        IEnumerable<PrimitiveEvent> Get(Guid id);
        long Save(PrimitiveEvent primitiveEvent);
        long GetSequenceNumber(Guid id);
    }
}