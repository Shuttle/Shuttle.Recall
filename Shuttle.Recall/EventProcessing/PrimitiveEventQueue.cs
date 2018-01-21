using System.Collections.Generic;
using Shuttle.Core.Contract;

namespace Shuttle.Recall
{
    public class PrimitiveEventQueue : IPrimitiveEventQueue
    {
        private static readonly object Lock = new object();

        private readonly Dictionary<string, Queue<PrimitiveEvent>> _queues =
            new Dictionary<string, Queue<PrimitiveEvent>>();

        public void EnqueueRange(string projectionName, IEnumerable<PrimitiveEvent> primitiveEvents)
        {
            Guard.AgainstNull(primitiveEvents, nameof(primitiveEvents));

            lock (Lock)
            {
                foreach (var primitiveEvent in primitiveEvents)
                {
                    Enqueue(projectionName, primitiveEvent);
                }
            }
        }

        public PrimitiveEvent Dequeue(string projectionName)
        {
            Guard.AgainstNullOrEmptyString(projectionName, "projectionName");

            lock (Lock)
            {
                if (!_queues.ContainsKey(projectionName))
                {
                    return null;
                }

                var queue = _queues[projectionName];

                if (queue.Count == 0)
                {
                    return null;
                }

                return queue.Dequeue();
            }
        }

        public void Enqueue(string projectionName, PrimitiveEvent primitiveEvent)
        {
            Guard.AgainstNullOrEmptyString(projectionName, "projectionName");
            Guard.AgainstNull(primitiveEvent, nameof(primitiveEvent));

            lock (Lock)
            {
                if (!_queues.ContainsKey(projectionName))
                {
                    _queues.Add(projectionName, new Queue<PrimitiveEvent>());
                }

                _queues[projectionName].Enqueue(primitiveEvent);
            }
        }
    }
}