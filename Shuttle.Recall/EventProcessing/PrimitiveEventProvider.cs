using System.Collections.Concurrent;
using System.Collections.Generic;
using Shuttle.Core.Contract;

namespace Shuttle.Recall
{
    public class PrimitiveEventProvider : IPrimitiveEventProvider
    {
        private readonly IEventStoreConfiguration _configuration;
        private readonly IPrimitiveEventRepository _repository;
        private readonly object Lock = new object();

        private readonly IDictionary<long, PrimitiveEvent> _primitiveEvents =
            new ConcurrentDictionary<long, PrimitiveEvent>();

        public PrimitiveEventProvider(IEventStoreConfiguration configuration, IPrimitiveEventRepository repository)
        {
            Guard.AgainstNull(configuration, nameof(configuration));
            Guard.AgainstNull(repository, nameof(repository));

            _configuration = configuration;
            _repository = repository;
        }

        public bool IsEmpty => _primitiveEvents.Count == 0;

        public void Completed(long sequenceNumber)
        {
            lock (Lock)
            {
                _primitiveEvents.Remove(sequenceNumber);
            }
        }

        public PrimitiveEvent Get(Projection projection)
        {
            Guard.AgainstNull(projection, nameof(projection));

            lock (Lock)
            {
                if (!_primitiveEvents.ContainsKey(projection.SequenceNumber))
                {
                    foreach (var primitiveEvent in _repository.Get(projection.SequenceNumber, projection.EventTypes,
                        _configuration.ProjectionEventFetchCount))
                    {
                        _primitiveEvents.Add(primitiveEvent.SequenceNumber, primitiveEvent);
                    }
                }

                if (_primitiveEvents.ContainsKey(projection.SequenceNumber))
                {
                    return _primitiveEvents[projection.SequenceNumber];
                }
            }

            return null;
        }
    }
}