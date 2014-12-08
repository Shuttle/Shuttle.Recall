using System;

namespace Shuttle.Recall.Core
{
	public interface IEventStore
	{
		EventStream Get(Guid id);
		void Save(EventStream eventStream);
	}
}