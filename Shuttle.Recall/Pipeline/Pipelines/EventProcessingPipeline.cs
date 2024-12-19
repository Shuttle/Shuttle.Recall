using System;
using System.Threading.Tasks;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Recall;

public class EventProcessingPipeline : Pipeline
{
    public EventProcessingPipeline(IServiceProvider serviceProvider, IProjectionEventObserver projectionEventObserver, IProjectionEventEnvelopeObserver projectionEventEnvelopeObserver, IHandleEventObserver handleEventObserver, IAcknowledgeEventObserver acknowledgeEventObserver) 
        : base(serviceProvider)
    {
        AddStage("Read")
            .WithEvent<OnGetEvent>()
            .WithEvent<OnAfterGetEvent>()
            .WithEvent<OnGetEventEnvelope>()
            .WithEvent<OnAfterGetEventEnvelope>();

        AddStage("Handle")
            .WithEvent<OnHandleEvent>()
            .WithEvent<OnAfterHandleEvent>()
            .WithEvent<OnAcknowledgeEvent>()
            .WithEvent<OnAfterAcknowledgeEvent>();

        AddObserver(Guard.AgainstNull(projectionEventObserver));
        AddObserver(Guard.AgainstNull(projectionEventEnvelopeObserver));
        AddObserver(Guard.AgainstNull(handleEventObserver));
        AddObserver(Guard.AgainstNull(acknowledgeEventObserver));

        AddObserver(async (IPipelineContext<OnPipelineException> context) =>
        {
            context.Pipeline.Abort();

            await Task.CompletedTask;
        });
    }
}