using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Recall
{
	public class EventStream
	{
		private readonly List<DomainEvent> _appendedEvents = new List<DomainEvent>();
		private readonly List<object> _events = new List<object>();
		private int _nextVersion;

		public EventStream(Guid id)
			: this(id, 0, null)
		{
			Id = id;
			Version = 0;
		}

		public EventStream(Guid id, int version, IEnumerable<object> events)
		{
			Id = id;
			Version = version;
			_nextVersion = version + 1;

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
				var method = instanceType.GetMethod(eventHandlingMethodName, new[] {@event.GetType()});

				if (method == null)
				{
					throw new UnhandledEventException(string.Format(RecallResources.UnhandledEventException,
						instanceType.AssemblyQualifiedName, eventHandlingMethodName, @event.GetType().AssemblyQualifiedName));
				}

				method.Invoke(instance, new[] {@event});
			}
		}

		public void Remove()
		{
			Removed = true;
		}

		public void ConcurrencyInvariant(int expectedVersion)
		{
			if (expectedVersion != Version)
			{
				throw new EventStreamConcurrencyException(string.Format(RecallResources.EventStreamConcurrencyException, Id, Version,
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