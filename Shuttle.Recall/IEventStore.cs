using System;

namespace Shuttle.Recall
{
	public interface IEventStore
	{
		EventStream Get(Guid id);
		EventStream GetRaw(Guid id);
		void Remove(Guid id);
		void SaveEventStream(EventStream eventStream);
	}
}