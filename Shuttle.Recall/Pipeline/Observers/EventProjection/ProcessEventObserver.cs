﻿using Shuttle.Core.Contract;
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
            var state = pipelineEvent.Pipeline.State;
            var projectionEvent = state.GetProjectionEvent();
            var eventEnvelope = state.GetEventEnvelope();
            var projection = state.GetProjection();
            var domainEvent = state.GetEvent();

            Guard.AgainstNull(projectionEvent, nameof(projectionEvent));

            if (!projectionEvent.HasPrimitiveEvent)
            {
                return;
            }

            Guard.AgainstNull(eventEnvelope, nameof(eventEnvelope));
            Guard.AgainstNull(projection, nameof(projection));
            Guard.AgainstNull(domainEvent, nameof(domainEvent));

            projection.Process(eventEnvelope, domainEvent.Event, projectionEvent.PrimitiveEvent, state.GetCancellationToken());
        }
    }
}