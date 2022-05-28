using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Recall
{
    public interface IProjectionEventObserver : IPipelineObserver<OnGetProjectionEvent>
    {
    }

    public class ProjectionEventObserver : IProjectionEventObserver
    {
        private readonly IProjectionEventProvider _provider;

        public ProjectionEventObserver(IProjectionEventProvider provider)
        {
            Guard.AgainstNull(provider, nameof(provider));

            _provider = provider;
        }

        public void Execute(OnGetProjectionEvent pipelineEvent)
        {
            var state = pipelineEvent.Pipeline.State;
            var projection = state.GetProjection();
            var projectionEvent = _provider.Get(projection);

            if (projectionEvent.HasPrimitiveEvent)
            {
                state.SetWorking();
            }

            state.SetProjectionEvent(projectionEvent);
        }
    }
}