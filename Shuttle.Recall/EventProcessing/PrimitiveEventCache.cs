using System;
using System.Collections.Generic;
using System.Runtime.Caching;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Recall
{
    public class PrimitiveEventCache : IPrimitiveEventCache
    {
        private static readonly object _padlock = new object();
        private readonly ObjectCache _cache = new MemoryCache("PrimitiveEvent");
        private readonly CacheItemPolicy _policy = new CacheItemPolicy();

        public PrimitiveEventCache()
        {
            _policy.SlidingExpiration = TimeSpan.FromSeconds(10);
        }

        public void AddRange(IEnumerable<PrimitiveEvent> primitiveEvents)
        {
            Guard.AgainstNull(primitiveEvents, "primitiveEvents");

            lock (_padlock)
            {
                foreach (var primitiveEvent in primitiveEvents)
                {
                    Add(primitiveEvent);
                }
            }
        }

        public PrimitiveEvent TryGet(long sequenceNumber)
        {
            lock (_padlock)
            {
                return (PrimitiveEvent)_cache.Get(sequenceNumber.ToString());
            }
        }

        public void Add(PrimitiveEvent primitiveEvent)
        {
            Guard.AgainstNull(primitiveEvent, "primitiveEvent");

            lock (_padlock)
            {
                var key = primitiveEvent.SequenceNumber.ToString();

                if (_cache.Contains(key))
                {
                    return;
                }

                _cache.Add(new CacheItem(key, primitiveEvent), _policy);
            }
        }
    }
}