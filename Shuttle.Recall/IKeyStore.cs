using System;

namespace Shuttle.Recall
{
	public interface IKeyStore
	{
		bool Contains(string key);
		Guid? Get(string key);
		void Remove(string key);
		void Remove(Guid id);
		void Add(Guid id, string key);
	}
}