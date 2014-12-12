using System;

namespace Shuttle.Recall.Core
{
	public interface IUniqueHashStore
	{
		Guid? Get(int indexType, string value);
		void Add(Guid id, int indexType, string value);
	}
}