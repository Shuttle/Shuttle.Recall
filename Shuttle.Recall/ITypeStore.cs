using System;

namespace Shuttle.Recall
{
	public interface ITypeStore
	{
		Guid Get(Type type);
		Guid Add(Type type);
	}
}