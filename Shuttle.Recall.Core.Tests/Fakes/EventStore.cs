using System;
using System.Collections.Generic;
using System.Linq;

namespace Shuttle.Recall.Core.Tests
{
	public class EventStore : IEventStore
	{
		private readonly Dictionary<Guid, EventStream> _eventStreams = new Dictionary<Guid, EventStream>();

		public EventStream Get(Guid id)
		{
			if (!_eventStreams.ContainsKey(id))
			{
				_eventStreams.Add(id, new EventStream(id, 0, new List<Event>()));
			}

			return _eventStreams[id];
		}

		public void Save(EventStream eventStream)
		{
			if (_eventStreams.ContainsKey(eventStream.Id))
			{
				_eventStreams.Remove(eventStream.Id);
			}

			var events = new List<Event>();

			events.AddRange(eventStream.PastEvents());
			events.AddRange(eventStream.NewEvents());

			_eventStreams.Add(eventStream.Id, new EventStream(eventStream.Id, eventStream.PastEvents().Count() + eventStream.NewEvents().Count(), events));
		}
	}
}