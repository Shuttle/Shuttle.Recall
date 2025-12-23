using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Recall;

public class EventProcessingPipeline : Pipeline
{
    public EventProcessingPipeline(IPipelineDependencies pipelineDependencies, IProjectionEventObserver projectionEventObserver, IProjectionEventEnvelopeObserver projectionEventEnvelopeObserver, IHandleEventObserver handleEventObserver, IAcknowledgeEventObserver acknowledgeEventObserver)
        : base(pipelineDependencies)
    {
        AddStage("Read")
            .WithEvent<RetrieveEvent>()
            .WithEvent<EventRetrieved>()
            .WithEvent<RetrieveEventEnvelope>()
            .WithEvent<EventEnvelopeRetrieved>();

        AddStage("Handle")
            .WithEvent<HandleEvent>()
            .WithEvent<EventHandled>()
            .WithEvent<AcknowledgeEvent>()
            .WithEvent<EventAcknowledged>();

        AddObserver(Guard.AgainstNull(projectionEventObserver));
        AddObserver(Guard.AgainstNull(projectionEventEnvelopeObserver));
        AddObserver(Guard.AgainstNull(handleEventObserver));
        AddObserver(Guard.AgainstNull(acknowledgeEventObserver));

        AddObserver(async (IPipelineContext<PipelineException> context) =>
        {
            context.Pipeline.Abort();

            await Task.CompletedTask;
        });
    }
}