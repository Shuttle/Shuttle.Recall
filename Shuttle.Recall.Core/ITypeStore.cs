using System;

namespace Shuttle.Recall.Core
{
	public interface ITypeStore
	{
		Guid Get(Type type);
		Guid Add(Type type);
	}
}