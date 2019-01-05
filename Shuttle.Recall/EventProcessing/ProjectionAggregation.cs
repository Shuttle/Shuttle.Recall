using System;
using System.Collections.Generic;
using System.Linq;
using Shuttle.Core.Contract;
using Shuttle.Core.Specification;

namespace Shuttle.Recall
{
    public class ProjectionAggregation : ISpecification<Projection>
    {
        private readonly int _projectionAggregationTolerance;

        private readonly Dictionary<string, Projection> _projections = new Dictionary<string, Projection>();

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

        public void Add(Projection projection)
        {
            Guard.AgainstNull(projection, nameof(projection));

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
    }
}