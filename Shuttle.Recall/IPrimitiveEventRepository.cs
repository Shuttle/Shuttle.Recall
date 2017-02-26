using System;
using System.Collections.Generic;

namespace Shuttle.Recall
{
    public interface IPrimitiveEventRepository
    {
        void Remove(Guid id);
        IEnumerable<PrimitiveEvent> Get(Guid id);
        IEnumerable<PrimitiveEvent> Get(long sequenceNumber, IEnumerable<Type> eventTypes, int limit);
        void Save(PrimitiveEvent primitiveEvent);
    }
}