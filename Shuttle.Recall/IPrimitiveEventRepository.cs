using System;
using System.Collections.Generic;

namespace Shuttle.Recall
{
    public interface IPrimitiveEventRepository
    {
        void Remove(Guid id);
        IEnumerable<PrimitiveEvent> Get(Guid id);
        IEnumerable<PrimitiveEvent> Get(long fromSequenceNumber, IEnumerable<Type> eventTypes, int limit);
        void Save(PrimitiveEvent primitiveEvent);
        void SaveSnapshot(PrimitiveEvent primitiveEvent);
    }
}