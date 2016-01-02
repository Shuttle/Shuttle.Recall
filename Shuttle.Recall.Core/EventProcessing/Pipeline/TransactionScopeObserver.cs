using System.Reflection;
using System.Transactions;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Recall.Core
{
	public class TransactionScopeObserver :
		IPipelineObserver<OnStartTransactionScope>,
		IPipelineObserver<OnCompleteTransactionScope>,
		IPipelineObserver<OnDisposeTransactionScope>,
		IPipelineObserver<OnAbortPipeline>,
		IPipelineObserver<OnPipelineException>
	{
		public void Execute(OnAbortPipeline pipelineEvent)
		{
			var state = pipelineEvent.Pipeline.State;
			var scope = state.Get<ITransactionScope>();

			if (scope == null)
			{
				return;
			}

			scope.Dispose();

			state.Replace<ITransactionScope>(null);
		}

		public void Execute(OnCompleteTransactionScope pipelineEvent)
		{
			var state = pipelineEvent.Pipeline.State;
			var scope = state.Get<ITransactionScope>();

			if (scope == null)
			{
				return;
			}

			if (pipelineEvent.Pipeline.Exception == null)
			{
				scope.Complete();
			}
		}

		public void Execute(OnDisposeTransactionScope pipelineEvent)
		{
			var state = pipelineEvent.Pipeline.State;
			var scope = state.Get<ITransactionScope>();

			if (scope == null)
			{
				return;
			}

			scope.Dispose();

			state.Replace<ITransactionScope>(null);
		}

		public void Execute(OnPipelineException pipelineEvent)
		{
			var state = pipelineEvent.Pipeline.State;
			var scope = state.Get<ITransactionScope>();

			if (scope == null)
			{
				return;
			}

			scope.Dispose();

			state.Replace<ITransactionScope>(null);
		}

		public void Execute(OnStartTransactionScope pipelineEvent)
		{
			var state = pipelineEvent.Pipeline.State;
			var scope = state.Get<ITransactionScope>();

			if (scope != null)
			{
				throw new TransactionException(
					string.Format(RecallResources.TransactionAlreadyStartedException, GetType().FullName,
						MethodBase.GetCurrentMethod().Name));
			}

			state.Replace<ITransactionScope>(new DefaultTransactionScope());
		}
	}
}