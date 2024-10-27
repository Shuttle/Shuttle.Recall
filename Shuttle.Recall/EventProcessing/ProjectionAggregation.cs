using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Shuttle.Core.Contract;
using Shuttle.Core.Specification;

namespace Shuttle.Recall;

public class ProjectionAggregation : ISpecification<Projection>
{
    private readonly CancellationToken _cancellationToken;
    private readonly List<Type> _eventTypes = new();
    private readonly SemaphoreSlim _externalLock = new(1, 1);
    private readonly object _lock = new();

    private readonly SortedDictionary<long, PrimitiveEvent> _primitiveEvents = new();

    private readonly int _projectionAggregationTolerance;
    private readonly Dictionary<string, Projection> _projections = new();
    private bool _initialized;
    private long _sequenceNumberTolerance = long.MinValue;

    public ProjectionAggregation(int projectionAggregationTolerance, CancellationToken cancellationToken)
    {
        _projectionAggregationTolerance = projectionAggregationTolerance;
        _cancellationToken = cancellationToken;
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

    public long SequenceNumberTail { get; private set; } = long.MaxValue;

    public bool IsSatisfiedBy(Projection candidate)
    {
        return Guard.AgainstNull(candidate).SequenceNumber < _sequenceNumberTolerance;
    }

    public void Add(Projection projection)
    {
        if (ContainsProjection(Guard.AgainstNull(projection).Name))
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

    public void AddPrimitiveEvent(PrimitiveEvent primitiveEvent)
    {
        lock (_lock)
        {
            if (_primitiveEvents.ContainsKey(Guard.AgainstNull(primitiveEvent).SequenceNumber))
            {
                return;
            }

            _primitiveEvents.Add(primitiveEvent.SequenceNumber, primitiveEvent);
        }
    }

    public bool ContainsPrimitiveEvent(long sequenceNumber)
    {
        return _primitiveEvents.ContainsKey(sequenceNumber);
    }

    public bool ContainsProjection(string name)
    {
        return _projections.ContainsKey(name);
    }

    public IEnumerable<Type> GetEventTypes()
    {
        lock (_lock)
        {
            if (!_initialized)
            {
                _eventTypes.AddRange(_projections.Values.SelectMany(projection => projection.EventTypes));

                _initialized = true;
            }
        }

        return _eventTypes;
    }

    public PrimitiveEvent? GetNextPrimitiveEvent(long sequenceNumber)
    {
        return _primitiveEvents.FirstOrDefault(entry => entry.Key > sequenceNumber).Value;
    }

    public async Task LockAsync()
    {
        await _externalLock.WaitAsync(_cancellationToken).ConfigureAwait(false);
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

    public void Release()
    {
        _externalLock.Release();
    }
}