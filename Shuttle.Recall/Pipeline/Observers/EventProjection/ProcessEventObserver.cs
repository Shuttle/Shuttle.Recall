using System;
using Shuttle.Core.Contract;
using Shuttle.Core.Logging;
using Shuttle.Core.Pipelines;

namespace Shuttle.Recall
{
    public interface IProcessEventObserver : IPipelineObserver<OnProcessEvent>
    {
    }

    public class ProcessEventObserver : IProcessEventObserver
    {
        private readonly ILog _log;

        public ProcessEventObserver()
        {
            _log = Log.For(this);
        }

        public void Execute(OnProcessEvent pipelineEvent)
        {
            var state = pipelineEvent.Pipeline.State;
            var projectionEvent = state.GetProjectionEvent();
            var eventEnvelope = state.GetEventEnvelope();
            var projection = state.GetProjection();
            var domainEvent = state.GetEvent();

            Guard.AgainstNull(projectionEvent, nameof(projectionEvent));

            if (!projectionEvent.HasPrimitiveEvent)
            {
                projection.Skip(projectionEvent.SequenceNumber);
                return;
            }

            Guard.AgainstNull(eventEnvelope, nameof(eventEnvelope));
            Guard.AgainstNull(projection, nameof(projection));
            Guard.AgainstNull(domainEvent, nameof(domainEvent));

            projection.Process(eventEnvelope, domainEvent, projectionEvent.PrimitiveEvent, state.GetCancellationToken());
        }
    }
}