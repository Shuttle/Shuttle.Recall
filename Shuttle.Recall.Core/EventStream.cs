using System.Collections.Generic;
using System.Linq;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Recall.Core
{
	public class EventStream
	{
		private readonly List<Event> _events = new List<Event>();
		private long _nextVersion;

		public EventStream(long version, IEnumerable<Event> events)
		{
			Version = version;
			_nextVersion = version + 1;

			if (events != null)
			{
				_events.AddRange(events);
			}
		}

		public long Version { get; private set; }

		public void Add(object data)
		{
			_events.Add(new Event(GetNextVersion(), data));
		}

		private long GetNextVersion()
		{
			var result = _nextVersion;

			_nextVersion = _nextVersion + 1;

			return result;
		}

		public IEnumerable<Event> NewEvents()
		{
			return _events.Where(e => e.Version > Version).ToList();
		}

		public void Apply(object instance)
		{
			foreach (var @event in _events.Where(e => e.Version <= Version))
			{
				var method = instance.GetType().GetMethod("Done", new[] { @event.GetType() });
			}
		}
	}
}