using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Recall
{
    public class EventStream
    {
        private readonly IEnumerable<object> _events;
        private readonly List<DomainEvent> _appendedEvents = new List<DomainEvent>();
        private int _nextVersion;

        public EventStream(Guid id)
        {
            Id = id;
            Version = 0;
        }

        public EventStream(Guid id, int version, IEnumerable<object> events)
        {
            Guard.AgainstNull(events,"events");

            Id = id;
            Version = version;
            _nextVersion = version + 1;

            _events = events;
        }

        public Guid Id { get; private set; }
        public int Version { get; private set; }
        public object Snapshot { get; private set; }
        
        public int Count {
            get { return (_events == null ? 0 : _events.Count()) + _appendedEvents.Count; }
        }

        public bool IsEmpty
        {
            get { return Count == 0; }
        }

        public EventStream AddEvent(object @event)
        {
            Guard.AgainstNull(@event, "@event");

            _appendedEvents.Add(new DomainEvent( @event, GetNextVersion()));

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
            Apply(instance, "On");
        }

        public void Apply(object instance, string eventHandlingMethodName)
        {
            Guard.AgainstNull(instance, "instance");

            if (_events == null)
            {
                return;
            }

            var instanceType = instance.GetType();

            foreach (var @event in _events)
            {
                var method = instanceType.GetMethod(eventHandlingMethodName, new[] { @event.GetType() });

                if (method == null)
                {
                    throw new UnhandledEventException(string.Format(RecallResources.UnhandledEventException,
                        instanceType.AssemblyQualifiedName, eventHandlingMethodName, @event.GetType().AssemblyQualifiedName));
                }

                method.Invoke(instance, new[] { @event });
            }
        }

        public bool HasSnapshot
        {
            get { return Snapshot != null; }
        }

        public void ConcurrencyInvariant(int expectedVersion)
        {
            if (expectedVersion != Version)
            {
                throw new EventStreamConcurrencyException(string.Format(RecallResources.EventStreamConcurrencyException, Id, Version, expectedVersion));
            }
        }

        public bool ShouldSave()
        {
            return _appendedEvents.Count > 0;
        }

        public IEnumerable<object> GetEvents()
        {
            return new ReadOnlyCollection<DomainEvent>(_appendedEvents);
        }
    }
}