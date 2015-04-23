using System;

namespace Shuttle.Recall.Core
{
	public interface IEventStore
	{
		EventStream Get(Guid id);
		EventStream GetRaw(Guid id);
		void SaveEventStream(EventStream eventStream);
	}
}