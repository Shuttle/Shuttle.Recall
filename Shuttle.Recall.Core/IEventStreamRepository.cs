using System;

namespace Shuttle.Recall.Core
{
	public interface IEventStreamRepository
	{
		EventStream Get(Guid id);
	}
}