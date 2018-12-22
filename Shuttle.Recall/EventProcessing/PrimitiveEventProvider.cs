using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Shuttle.Core.Contract;

namespace Shuttle.Recall
{
    public class PrimitiveEventProvider : IPrimitiveEventProvider
    {
        private readonly IEventStoreConfiguration _configuration;

        private readonly KeyValuePair<long, PrimitiveEvent> _default = default(KeyValuePair<long, PrimitiveEvent>);
        private readonly IEventProcessor _eventProcessor;
        private readonly object _lock = new object();

        private readonly IDictionary<long, PrimitiveEvent> _primitiveEvents =
            new ConcurrentDictionary<long, PrimitiveEvent>();

        private readonly IPrimitiveEventRepository _repository;

        private long _sequenceNumberHead;

        public PrimitiveEventProvider(IEventStoreConfiguration configuration, IEventProcessor eventProcessor,
            IPrimitiveEventRepository repository)
        {
            Guard.AgainstNull(configuration, nameof(configuration));
            Guard.AgainstNull(eventProcessor, nameof(eventProcessor));
            Guard.AgainstNull(repository, nameof(repository));

            _configuration = configuration;
            _eventProcessor = eventProcessor;
            _repository = repository;
        }

        public bool IsEmpty => _primitiveEvents.Count == 0;

        public void Completed(long sequenceNumber)
        {
            lock (_lock)
            {
                _primitiveEvents.Remove(sequenceNumber);
            }
        }

        public ProjectionEvent Get(Projection projection)
        {
            Guard.AgainstNull(projection, nameof(projection));

            lock (_lock)
            {
                if (!_primitiveEvents.ContainsKey(projection.SequenceNumber))
                {
                    foreach (var primitiveEvent in _repository.Get(projection.SequenceNumber,
                        _eventProcessor.EventTypes,
                        _configuration.ProjectionEventFetchCount))
                    {
                        _primitiveEvents.Add(primitiveEvent.SequenceNumber, primitiveEvent);

                        if (primitiveEvent.SequenceNumber > _sequenceNumberHead)
                        {
                            _sequenceNumberHead = primitiveEvent.SequenceNumber;
                        }
                    }
                }

                var result =
                    _primitiveEvents.FirstOrDefault(entry => entry.Value.SequenceNumber > projection.SequenceNumber);

                return result.Equals(_default)
                    ? new ProjectionEvent(_sequenceNumberHead)
                    : new ProjectionEvent(result.Value);
            }
        }
    }
}