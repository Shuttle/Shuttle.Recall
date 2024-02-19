using System.Threading.Tasks;
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
            ExecuteAsync(pipelineEvent).GetAwaiter().GetResult();
        }

        public async Task ExecuteAsync(OnGetProjectionEvent pipelineEvent)
        {
            var state = Guard.AgainstNull(pipelineEvent, nameof(pipelineEvent)).Pipeline.State;
            var projection = Guard.AgainstNull(state.GetProjection(), StateKeys.Projection);
            var projectionEvent = _provider.Get(projection);

            if (projectionEvent.HasPrimitiveEvent)
            {
                state.SetWorking();
            }

            state.SetProjectionEvent(projectionEvent);

            await Task.CompletedTask;
        }
    }
}