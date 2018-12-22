using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Recall
{
    public interface IProjectionPrimitiveEventObserver : IPipelineObserver<OnGetProjectionEvent>
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

        public void Execute(OnGetProjectionEvent pipelineEvent)
        {
            var state = pipelineEvent.Pipeline.State;
            var projection = state.GetProjection();
            var projectionEvent = _provider.Get(projection);

            state.SetProjectionEvent(projectionEvent);
        }
    }
}