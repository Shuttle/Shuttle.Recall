using System;

namespace Shuttle.Recall
{
	public interface ITransactionScope : IDisposable
	{
		void Complete();
	}
}