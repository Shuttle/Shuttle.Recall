using System;
using System.Collections.Generic;
using System.Linq;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Recall.Core
{
	public class EventStream
	{
		private readonly List<Event> _events = new List<Event>();
		private int _nextVersion;

		public EventStream(Guid id, int version, IEnumerable<Event> events)
		{
			Id = id;
			Version = version;
			_nextVersion = version + 1;

			if (events != null)
			{
				_events.AddRange(events);
			}
		}

		public Guid Id { get; private set; }
		public long Version { get; private set; }

		public void Add(object data)
		{
			_events.Add(new Event(GetNextVersion(), data.GetType().AssemblyQualifiedName, data));
		}

		private int GetNextVersion()
		{
			var result = _nextVersion;

			_nextVersion = _nextVersion + 1;

			return result;
		}

		public IEnumerable<Event> NewEvents()
		{
			return _events.Where(e => e.Version > Version);
		}

		public IEnumerable<Event> PastEvents()
		{
			return _events.Where(e => e.Version <= Version);
		}

		public void Apply(object instance)
		{
			Apply(instance, "Done");
		}

		public void Apply(object instance, string eventHandlingMethodName)
		{
			Guard.AgainstNull(instance, "instance");

			foreach (var @event in PastEvents())
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
	}
}