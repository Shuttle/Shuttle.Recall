using System;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Recall.Core
{
	public class EventStore
	{
		private readonly IEventStreamRepository _eventStreamRepository;

		public EventStore(IEventStreamRepository eventStreamRepository)
		{
			Guard.AgainstNull(eventStreamRepository, "eventStreamRepository");

			_eventStreamRepository = eventStreamRepository;
		}

		public EventStream Get<T>(Guid id, T instance) where T : class
		{
			Guard.AgainstNull(instance, "instance");

			var result = _eventStreamRepository.Get(id);

			result.Apply(instance);

			return result;
		}
	}
}