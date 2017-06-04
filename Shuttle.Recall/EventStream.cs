using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Recall
{
    public class EventStream
    {
        private readonly IEventMethodInvoker _eventMethodInvoker;
        private readonly List<DomainEvent> _appendedEvents = new List<DomainEvent>();
        private readonly List<object> _events = new List<object>();
        private int _nextVersion;

        public EventStream(Guid id, IEventMethodInvoker eventMethodInvoker)
            : this(id, 0, null, eventMethodInvoker)
        {
            Id = id;
            Version = 0;
        }

        public EventStream(Guid id, int version, IEnumerable<object> events, IEventMethodInvoker eventMethodInvoker)
        {
            Guard.AgainstNull(eventMethodInvoker, "eventMethodInvoker");

            Id = id;
            Version = version;
            _nextVersion = version + 1;
            _eventMethodInvoker = eventMethodInvoker;

            if (events != null)
            {
                _events.AddRange(events);
            }
        }

        public Guid Id { get; }
        public int Version { get; }
        public object Snapshot { get; private set; }

        public int Count
        {
            get { return (_events == null ? 0 : _events.Count()) + _appendedEvents.Count; }
        }

        public bool IsEmpty
        {
            get { return Count == 0; }
        }

        public bool HasSnapshot
        {
            get { return Snapshot != null; }
        }

        public bool Removed { get; private set; }

        public EventStream AddEvent(object @event)
        {
            Guard.AgainstNull(@event, "@event");

            _appendedEvents.Add(new DomainEvent(@event, GetNextVersion()));

            return this;
        }

        private int GetNextVersion()
        {
            var result = _nextVersion;

            _nextVersion = _nextVersion + 1;

            return result;
        }

        public EventStream AddSnapshot(object snapshot)
        {
            Guard.AgainstNull(snapshot, "snapshot");

            Snapshot = snapshot;

            _appendedEvents.Add(new DomainEvent(snapshot, GetNextVersion()).AsSnapshot());

            return this;
        }

        public void Apply(object instance)
        {
            Guard.AgainstNull(instance, "instance");

            _eventMethodInvoker.Apply(instance, _events);
        }

        public void Remove()
        {
            Removed = true;
        }

        public void ConcurrencyInvariant(int expectedVersion)
        {
            if (expectedVersion != Version)
            {
                throw new EventStreamConcurrencyException(string.Format(
                    RecallResources.EventStreamConcurrencyException, Id, Version,
                    expectedVersion));
            }
        }

        public bool ShouldSave()
        {
            return _appendedEvents.Count > 0;
        }

        public IEnumerable<DomainEvent> GetEvents()
        {
            return new ReadOnlyCollection<DomainEvent>(_appendedEvents);
        }

        /// <summary>
        ///     Appended events are moved to the events collection since they have been committed but can still be applied to any
        ///     other aggregate.
        /// </summary>
        public void Commit()
        {
            _events.AddRange(_appendedEvents);
            _appendedEvents.Clear();
        }
    }
}