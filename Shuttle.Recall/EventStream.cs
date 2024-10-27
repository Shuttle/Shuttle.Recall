using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Shuttle.Core.Contract;

namespace Shuttle.Recall;

public class EventStream
{
    [Flags]
    public enum EventRegistrationType
    {
        Committed = 1 << 0,
        Appended = 1 << 1,
        All = Committed + Appended
    }

    private readonly List<DomainEvent> _appendedEvents = new();
    private readonly IEventMethodInvoker _eventMethodInvoker;
    private readonly List<DomainEvent> _events = new();
    private int _nextVersion;

    public EventStream(Guid id, IEventMethodInvoker eventMethodInvoker)
        : this(id, 0, eventMethodInvoker, null)
    {
        Id = id;
        Version = 0;
    }

    public EventStream(Guid id, int version, IEventMethodInvoker eventMethodInvoker, IEnumerable<DomainEvent>? events = null)
    {
        Id = id;
        Version = version;
        _nextVersion = version + 1;
        _eventMethodInvoker = Guard.AgainstNull(eventMethodInvoker);

        if (events != null)
        {
            _events.AddRange(events);
        }
    }

    public int Count => (_events?.Count ?? 0) + _appendedEvents.Count;
    public Guid Id { get; }
    public Guid? CorrelationId { get; private set; }
    public bool IsEmpty => Count == 0;
    public bool Removed { get; private set; }
    public int Version { get; }

    public EventStream WithCorrelationId(Guid correlationId)
    {
        if (CorrelationId.HasValue)
        {
            throw new InvalidOperationException(string.Format(Resources.EventStreamCorrelationIdAlreadySetException, Id, CorrelationId));
        }

        CorrelationId = correlationId;

        return this;
    }

    public EventStream Add(object @event)
    {
        _appendedEvents.Add(new(Guard.AgainstNull(@event), GetNextVersion()));

        return this;
    }

    public void Apply(object instance)
    {
        _eventMethodInvoker.Apply(Guard.AgainstNull(instance), _events.Select(domainEvent => domainEvent.Event));
    }

    public void Commit()
    {
        _events.AddRange(_appendedEvents);
        _appendedEvents.Clear();
    }

    public void ConcurrencyInvariant(int expectedVersion)
    {
        if (expectedVersion != Version)
        {
            throw new EventStreamConcurrencyException(string.Format(
                Resources.EventStreamConcurrencyException, Id, Version,
                expectedVersion));
        }
    }

    public IEnumerable<DomainEvent> GetEvents(EventRegistrationType type = EventRegistrationType.Appended)
    {
        var result = new List<DomainEvent>();

        if (type.HasFlag(EventRegistrationType.Appended))
        {
            result.AddRange(_appendedEvents);
        }

        if (type.HasFlag(EventRegistrationType.Committed))
        {
            result.AddRange(_events);
        }

        return new ReadOnlyCollection<DomainEvent>(result);
    }

    private int GetNextVersion()
    {
        var result = _nextVersion;

        _nextVersion += 1;

        return result;
    }

    public void Remove()
    {
        Removed = true;
    }

    public bool ShouldSave()
    {
        return _appendedEvents.Count > 0;
    }
}