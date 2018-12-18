using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Recall
{
    public interface IProjectionPrimitiveEventObserver : IPipelineObserver<OnGetProjectionPrimitiveEvent>
    {
    }

    public class ProjectionPrimitiveEventObserver : IProjectionPrimitiveEventObserver
    {
        private readonly IPrimitiveEventProvider _provider;

        public ProjectionPrimitiveEventObserver(IPrimitiveEventProvider provider)
        {
            Guard.AgainstNull(provider, nameof(provider));

            _provider = provider;
        }

        public void Execute(OnGetProjectionPrimitiveEvent pipelineEvent)
        {
            var state = pipelineEvent.Pipeline.State;
            var projection = state.GetProjection();

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