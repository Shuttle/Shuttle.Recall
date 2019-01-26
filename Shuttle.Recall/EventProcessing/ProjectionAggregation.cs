using System;
using System.Collections.Generic;
using System.Linq;
using Shuttle.Core.Contract;
using Shuttle.Core.Specification;

namespace Shuttle.Recall
{
    public class ProjectionAggregation : ISpecification<Projection>
    {
        private readonly object _lock = new object();

        private readonly IDictionary<long, PrimitiveEvent> _primitiveEvents = new Dictionary<long, PrimitiveEvent>();
        private readonly int _projectionAggregationTolerance;
        private readonly Dictionary<string, Projection> _projections = new Dictionary<string, Projection>();
        private long _sequenceNumberTolerance = long.MinValue;

        public ProjectionAggregation(int projectionAggregationTolerance)
        {
            _projectionAggregationTolerance = projectionAggregationTolerance;
        }

        public long SequenceNumberTail { get; private set; } = long.MaxValue;
        public IEnumerable<Type> EventTypes { get; private set; } = new List<Type>();

        public Guid Id { get; } = Guid.NewGuid();

        public bool IsEmpty => _primitiveEvents.Count == 0;

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

            var eventTypes = new List<Type>(EventTypes);

            foreach (var type in projection.EventTypes)
            {
                if (eventTypes.Contains(type))
                {
                    continue;
                }

                eventTypes.Add(type);
            }

            EventTypes = eventTypes.AsReadOnly();
        }

        public long TrimSequenceNumberTail()
        {
            SequenceNumberTail = _projections.Min(item => item.Value.SequenceNumber);

            return SequenceNumberTail;
        }

        public void ProcessSequenceNumberTail()
        {
            lock (_lock)
            {
                var sequenceNumberTail = _projections
                    .Min(pair => pair.Value.SequenceNumber);
                var keys = _primitiveEvents
                    .Where(pair => pair.Value.SequenceNumber <= sequenceNumberTail)
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

            if (_primitiveEvents.ContainsKey(primitiveEvent.SequenceNumber))
            {
                return;
            }

            _primitiveEvents.Add(primitiveEvent.SequenceNumber, primitiveEvent);
        }

        public PrimitiveEvent GetNextPrimitiveEvent(long sequenceNumber)
        {
            return _primitiveEvents.FirstOrDefault(entry => entry.Value.SequenceNumber > sequenceNumber).Value;
        }
    }
}