using System.Collections.Generic;

namespace Shuttle.Recall
{
    public interface IPrimitiveEventCache
    {
        void AddRange(IEnumerable<PrimitiveEvent> primitiveEvents);
        PrimitiveEvent TryGet(long sequenceNumber);
        void Add(PrimitiveEvent primitiveEvent);
    }
}