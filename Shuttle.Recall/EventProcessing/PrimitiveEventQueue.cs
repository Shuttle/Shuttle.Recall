using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Recall
{
    public class PrimitiveEventQueue : IPrimitiveEventQueue
    {
        private static readonly object _padlock = new object();
        private readonly Dictionary<string, Queue<PrimitiveEvent>> _queues = new Dictionary<string, Queue<PrimitiveEvent>>();

        public void EnqueueRange(string projectionName, IEnumerable<PrimitiveEvent> primitiveEvents)
        {
            Guard.AgainstNull(primitiveEvents, "primitiveEvents");

            lock (_padlock)
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

            lock (_padlock)
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
            Guard.AgainstNull(primitiveEvent, "primitiveEvent");

            lock (_padlock)
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