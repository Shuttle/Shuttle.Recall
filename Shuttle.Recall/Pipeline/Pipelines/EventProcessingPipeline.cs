using System;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Recall;

public class EventProcessingPipeline : Pipeline
{
    public EventProcessingPipeline(IServiceProvider serviceProvider, IProjectionEventObserver projectionEventObserver, IProjectionEventEnvelopeObserver projectionEventEnvelopeObserver, IHandleEventObserver handleEventObserver, IAcknowledgeEventObserver acknowledgeEventObserver) 
        : base(serviceProvider)
    {
        RegisterStage("EventProcessing.Read")
            .WithEvent<OnGetProjectionEvent>()
            .WithEvent<OnAfterGetProjectionEvent>()
            .WithEvent<OnGetProjectionEventEnvelope>()
            .WithEvent<OnAfterGetProjectionEventEnvelope>();

        RegisterStage("EventProcessing.Handle")
            .WithEvent<OnHandleEvent>()
            .WithEvent<OnAfterHandleEvent>()
            .WithEvent<OnAcknowledgeEvent>()
            .WithEvent<OnAfterAcknowledgeEvent>();

        RegisterObserver(Guard.AgainstNull(projectionEventObserver));
        RegisterObserver(Guard.AgainstNull(projectionEventEnvelopeObserver));
        RegisterObserver(Guard.AgainstNull(handleEventObserver));
        RegisterObserver(Guard.AgainstNull(acknowledgeEventObserver));
    }
}