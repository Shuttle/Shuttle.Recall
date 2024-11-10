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
        AddStage("EventProcessing.Read")
            .WithEvent<OnGetProjectionEvent>()
            .WithEvent<OnAfterGetProjectionEvent>()
            .WithEvent<OnGetProjectionEventEnvelope>()
            .WithEvent<OnAfterGetProjectionEventEnvelope>();

        AddStage("EventProcessing.Handle")
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
            await Task.CompletedTask;

            Console.WriteLine(@$"[UNRECOVERABLE] : {context.Pipeline.Exception?.ToString() ?? "(unknown exception)"}");

            Environment.Exit(1);
        });
    }
}