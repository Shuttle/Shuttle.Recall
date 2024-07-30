using System.Threading.Tasks;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Recall
{
    public interface IProcessEventObserver : IPipelineObserver<OnProcessEvent>
    {
    }

    public class ProcessEventObserver : IProcessEventObserver
    {
        public void Execute(OnProcessEvent pipelineEvent)
        {
            ExecuteAsync(pipelineEvent, true).GetAwaiter().GetResult();
        }

        public async Task ExecuteAsync(OnProcessEvent pipelineEvent)
        {
            await ExecuteAsync(pipelineEvent, false).ConfigureAwait(false);
        }

        private async Task ExecuteAsync(OnProcessEvent pipelineEvent, bool sync)
        {
            var state = Guard.AgainstNull(pipelineEvent, nameof(pipelineEvent)).Pipeline.State;
            var projectionEvent = Guard.AgainstNull(state.GetProjectionEvent(), StateKeys.ProjectionEvent);

            if (!projectionEvent.HasPrimitiveEvent)
            {
                return;
            }

            var eventEnvelope = Guard.AgainstNull(state.GetEventEnvelope(), StateKeys.EventEnvelopes);
            var projection = Guard.AgainstNull(state.GetProjection(), StateKeys.Projection);
            var domainEvent = Guard.AgainstNull(state.GetEvent(), StateKeys.Event);

            if (sync)
            {
                projection.Process(eventEnvelope, domainEvent.Event, projectionEvent.PrimitiveEvent, pipelineEvent.Pipeline.CancellationToken);
            }
            else
            {
                await projection.ProcessAsync(eventEnvelope, domainEvent.Event, projectionEvent.PrimitiveEvent, pipelineEvent.Pipeline.CancellationToken);
            }
        }
    }
}