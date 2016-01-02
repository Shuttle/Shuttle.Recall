using System;

namespace Shuttle.Recall.Core
{
	public interface ITransactionScope : IDisposable
	{
		void Complete();
	}
}