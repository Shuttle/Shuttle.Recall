using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Recall;

public class EventProcessingPipeline : Pipeline
{
    public EventProcessingPipeline(IPipelineDependencies pipelineDependencies, IProjectionEventObserver projectionEventObserver, IProjectionEventEnvelopeObserver projectionEventEnvelopeObserver, IHandleEventObserver handleEventObserver, IAcknowledgeEventObserver acknowledgeEventObserver, IEventProcessingPipelineFailedObserver eventProcessingPipelineFailedObserver)
        : base(pipelineDependencies)
    {
        AddStage("Handle")
            .WithEvent<RetrieveEvent>()
            .WithEvent<EventRetrieved>()
            .WithEvent<RetrieveEventEnvelope>()
            .WithEvent<EventEnvelopeRetrieved>()
            .WithEvent<HandleEvent>()
            .WithEvent<EventHandled>()
            .WithEvent<AcknowledgeEvent>()
            .WithEvent<EventAcknowledged>()
            .WithEvent<CompleteTransactionScope>()
            .WithEvent<DisposeTransactionScope>();

        AddObserver(Guard.AgainstNull(projectionEventObserver));
        AddObserver(Guard.AgainstNull(projectionEventEnvelopeObserver));
        AddObserver(Guard.AgainstNull(handleEventObserver));
        AddObserver(Guard.AgainstNull(acknowledgeEventObserver));
        AddObserver(Guard.AgainstNull(eventProcessingPipelineFailedObserver));

        AddObserver(async (IPipelineContext<PipelineFailed> context) =>
        {
            context.Pipeline.Abort();

            await Task.CompletedTask;
        });
    }
}