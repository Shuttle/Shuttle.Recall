﻿using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Recall
{
    public class EventProcessingPipeline : Pipeline
    {
        public EventProcessingPipeline(IProjectionEventObserver projectionEventObserver, IProjectionEventEnvelopeObserver projectionEventEnvelopeObserver, IProcessEventObserver processEventObserver, IAcknowledgeEventObserver acknowledgeEventObserver) 
        {
            RegisterStage("EventProcessing.Read")
                .WithEvent<OnGetProjectionEvent>()
                .WithEvent<OnAfterGetProjectionEvent>()
                .WithEvent<OnGetProjectionEventEnvelope>()
                .WithEvent<OnAfterGetProjectionEventEnvelope>();

            RegisterStage("EventProcessing.Handle")
                .WithEvent<OnProcessEvent>()
                .WithEvent<OnAfterProcessEvent>()
                .WithEvent<OnAcknowledgeEvent>()
                .WithEvent<OnAfterAcknowledgeEvent>();

            RegisterObserver(Guard.AgainstNull(projectionEventObserver, nameof(projectionEventObserver)));
            RegisterObserver(Guard.AgainstNull(projectionEventEnvelopeObserver, nameof(projectionEventEnvelopeObserver)));
            RegisterObserver(Guard.AgainstNull(processEventObserver, nameof(processEventObserver)));
            RegisterObserver(Guard.AgainstNull(acknowledgeEventObserver, nameof(acknowledgeEventObserver)));
        }
    }
}