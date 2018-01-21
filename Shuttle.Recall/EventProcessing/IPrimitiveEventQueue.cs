using System.Collections.Generic;

namespace Shuttle.Recall
{
    public interface IPrimitiveEventQueue
    {
        void EnqueueRange(string projectionName, IEnumerable<PrimitiveEvent> primitiveEvents);
        PrimitiveEvent Dequeue(string projectionName);
        void Enqueue(string projectionName, PrimitiveEvent primitiveEvent);
    }
}