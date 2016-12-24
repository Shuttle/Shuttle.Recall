using Shuttle.Core.Infrastructure;

namespace Shuttle.Recall
{
	public class GetProjectionPrimitiveEventObserver : IPipelineObserver<OnGetProjectionPrimitiveEvent>
	{
	    private readonly IPrimitiveEventProvider _provider;

	    public GetProjectionPrimitiveEventObserver(IPrimitiveEventProvider provider)
	    {
	        Guard.AgainstNull(provider, "provider");

            _provider = provider;
	    }

        public void Execute(OnGetProjectionPrimitiveEvent pipelineEvent)
		{
			var state = pipelineEvent.Pipeline.State;
			var projection = state.GetEventProjection();

            var primitiveEvent = _provider.Get(projection);

			if (primitiveEvent == null)
			{
				pipelineEvent.Pipeline.Abort();
			}
			else
			{
				state.SetWorking();
				state.SetPrimitiveEvent(primitiveEvent);
			}
		}
	}
}