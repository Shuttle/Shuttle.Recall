using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Shuttle.Core.Contract;
using Shuttle.Core.Specification;

namespace Shuttle.Recall
{
    public class ProjectionAggregation : ISpecification<Projection>
    {
        private readonly SemaphoreSlim _externalLock = new SemaphoreSlim(1, 1);
        private readonly object _lock = new object();

        private readonly SortedDictionary<long, PrimitiveEvent> _primitiveEvents = new SortedDictionary<long, PrimitiveEvent>();

        private readonly int _projectionAggregationTolerance;
        private readonly CancellationToken _cancellationToken;
        private readonly Dictionary<string, Projection> _projections = new Dictionary<string, Projection>();
        private long _sequenceNumberTolerance = long.MinValue;
        private readonly List<Type> _eventTypes = new List<Type>();
        private bool _initialized;

        public ProjectionAggregation(int projectionAggregationTolerance, CancellationToken cancellationToken)
        {
            _projectionAggregationTolerance = projectionAggregationTolerance;
            _cancellationToken = cancellationToken;
        }

        public long SequenceNumberTail { get; private set; } = long.MaxValue;

        public IEnumerable<Type> GetEventTypes()
        {
            lock(_lock)
            {
                if (!_initialized)
                {
                    _eventTypes.AddRange(_projections.Values.SelectMany(projection => projection.EventTypes));

                    _initialized = true;
                }
            }

            return _eventTypes;
        }

        public Guid Id { get; } = Guid.NewGuid();

        public bool IsEmpty
        {
            get
            {
                lock (_lock)
                {
                    return _primitiveEvents.Count == 0;
                }
            }
        }

        public bool IsSatisfiedBy(Projection candidate)
        {
            Guard.AgainstNull(candidate, nameof(candidate));

            return candidate.SequenceNumber < _sequenceNumberTolerance;
        }

        public bool ContainsProjection(string name)
        {
            return _projections.ContainsKey(name);
        }

        public void Add(Projection projection)
        {
            Guard.AgainstNull(projection, nameof(projection));

            if (ContainsProjection(projection.Name))
            {
                return;
            }

            _projections.Add(projection.Name, projection);

            if (projection.SequenceNumber < SequenceNumberTail)
            {
                SequenceNumberTail = projection.SequenceNumber;
                _sequenceNumberTolerance = SequenceNumberTail + _projectionAggregationTolerance;
            }

            projection.Aggregate(Id);
        }

        public void ProcessSequenceNumberTail()
        {
            lock (_lock)
            {
                SequenceNumberTail = _projections
                    .Min(pair => pair.Value.SequenceNumber);

                var keys = _primitiveEvents
                    .Where(pair => pair.Value.SequenceNumber <= SequenceNumberTail)
                    .Select(pair => pair.Key)
                    .ToList();

                foreach (var key in keys)
                {
                    _primitiveEvents.Remove(key);
                }
            }
        }

        public bool ContainsPrimitiveEvent(long sequenceNumber)
        {
            return _primitiveEvents.ContainsKey(sequenceNumber);
        }

        public void AddPrimitiveEvent(PrimitiveEvent primitiveEvent)
        {
            Guard.AgainstNull(primitiveEvent, nameof(primitiveEvent));

            lock (_lock)
            {
                if (_primitiveEvents.ContainsKey(primitiveEvent.SequenceNumber))
                {
                    return;
                }

                _primitiveEvents.Add(primitiveEvent.SequenceNumber, primitiveEvent);
            }
        }

        public PrimitiveEvent GetNextPrimitiveEvent(long sequenceNumber)
        {
            return _primitiveEvents.FirstOrDefault(entry => entry.Key > sequenceNumber).Value;
        }

        public async Task LockAsync()
        {
            await _externalLock.WaitAsync(_cancellationToken).ConfigureAwait(false);
        }

        public void Release()
        {
            _externalLock.Release();
        }
    }
}