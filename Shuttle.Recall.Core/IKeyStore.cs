using System;

namespace Shuttle.Recall.Core
{
	public interface IKeyStore
	{
		Guid? Get(string key);
	    bool Contains(string key);
		void Remove(string key);
		void Remove(Guid id);
		void Add(Guid id, string key);
	}
}