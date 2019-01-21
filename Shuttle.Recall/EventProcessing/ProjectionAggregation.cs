using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Shuttle.Core.Contract;
using Shuttle.Core.Specification;

namespace Shuttle.Recall
{
    public class ProjectionAggregation : ISpecification<Projection>
    {
        private readonly int _projectionAggregationTolerance;

        private readonly IDictionary<long, PrimitiveEvent> _primitiveEvents = new Dictionary<long, PrimitiveEvent>();
        private readonly Dictionary<string, Projection> _projections = new Dictionary<string, Projection>();
        private readonly object _lock = new object();

        public long SequenceNumberTail { get; private set; } = long.MaxValue;
        private long _sequenceNumberTolerance = long.MinValue;

        public ProjectionAggregation(int projectionAggregationTolerance)
        {
            _projectionAggregationTolerance = projectionAggregationTolerance;
        }

        public Guid Id { get; } = Guid.NewGuid();

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

        public long TrimSequenceNumberTail()
        {
            SequenceNumberTail = _projections.Min(item => item.Value.SequenceNumber);

            return SequenceNumberTail;
        }

        public bool IsEmpty => _primitiveEvents.Count == 0;

        public void Completed(long sequenceNumber)
        {
            lock (_lock)
            {
                _primitiveEvents.Remove(sequenceNumber);
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