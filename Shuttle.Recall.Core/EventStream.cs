using System;
using System.Collections.Generic;
using System.Linq;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Recall.Core
{
	public class EventStream
	{
		private readonly List<Event> _events = new List<Event>();
		private readonly int _initialVersion;

		public EventStream(Guid id, int version, IEnumerable<Event> events, Event snapshot)
		{
			Id = id;
			Version = version;
			_initialVersion = version;
			Snapshot = snapshot;

			if (events != null)
			{
				_events.AddRange(events);
			}
		}

		public Guid Id { get; private set; }
		public int Version { get; private set; }
		public Event Snapshot { get; private set; }

		public void AddEvent(object data)
		{
			Guard.AgainstNull(data, "data");

			Version = Version + 1;

			_events.Add(new Event(Version, data.GetType().AssemblyQualifiedName, data));
		}

		public void AddSnapshot(object data)
		{
			Guard.AgainstNull(data, "data");

			Snapshot = new Event(Version, data.GetType().AssemblyQualifiedName, data);
		}

		public bool ShouldSnapshot(int minimumEventCount)
		{
			return _events.Count >= minimumEventCount;
		}

		public IEnumerable<Event> EventsAfter(Event @event)
		{
			return _events.Where(e => e.Version > @event.Version);
		}

		public IEnumerable<Event> EventsAfter(int version)
		{
			return _events.Where(e => e.Version > version);
		}

		public IEnumerable<Event> NewEvents()
		{
			return _events.Where(e => e.Version > _initialVersion);
		}

		public IEnumerable<Event> PastEvents()
		{
			return _events.Where(e => e.Version <= _initialVersion);
		}

		public void Apply(object instance)
		{
			Apply(instance, "Done");
		}

		public void Apply(object instance, string eventHandlingMethodName)
		{
			Guard.AgainstNull(instance, "instance");

			var events = new List<Event>(PastEvents());

			if (HasSnapshot)
			{
				events.Insert(0, Snapshot);
			}

			foreach (var @event in events)
			{
				var method = instance.GetType().GetMethod(eventHandlingMethodName, new[] {@event.Data.GetType()});

				if (method == null)
				{
					throw new UnhandledEventException(string.Format(RecallResources.UnhandledEventException,
						instance.GetType().AssemblyQualifiedName, eventHandlingMethodName, @event.GetType().AssemblyQualifiedName));
				}

				method.Invoke(instance, new[] {@event.Data});
			}
		}

		public bool HasSnapshot
		{
			get { return Snapshot != null; }
		}

		public void ConcurrencyInvariant(int expectedVersion)
		{
			if (expectedVersion != _initialVersion)
			{
				throw new EventStreamConcurrencyException(string.Format(RecallResources.EventStreamConcurrencyException, Id, _initialVersion, expectedVersion));
			}
		}
	}
}