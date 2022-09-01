using System;
using System.Collections.Generic;

namespace Shuttle.Recall
{
    public interface IPrimitiveEventRepository
    {
        void Remove(Guid id);
        IEnumerable<PrimitiveEvent> Get(Guid id);
        IEnumerable<PrimitiveEvent> Fetch(long fromSequenceNumber, int count, IEnumerable<Type> eventTypes);
        long Save(PrimitiveEvent primitiveEvent);
        long GetSequenceNumber(Guid id);
    }
}