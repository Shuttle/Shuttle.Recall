using Microsoft.Extensions.Options;
using Shuttle.Contract;
using Shuttle.Pipelines;

namespace Shuttle.Recall;

public interface IEventProcessingPipeline : IPipeline;

public class EventProcessingPipeline : Pipeline, IEventProcessingPipeline
{
    public EventProcessingPipeline(IOptions<PipelineOptions> pipelineOptions, IServiceProvider serviceProvider, IProjectionEventObserver projectionEventObserver, IProjectionEventEnvelopeObserver projectionEventEnvelopeObserver, IHandleEventObserver handleEventObserver, IAcknowledgeEventObserver acknowledgeEventObserver, IEventProcessingPipelineFailedObserver eventProcessingPipelineFailedObserver)
        : base(pipelineOptions, serviceProvider)
    {
        AddStage("Handle")
            .WithEvent<RetrieveEvent>()
            .WithEvent<EventRetrieved>()
            .WithEvent<RetrieveEventEnvelope>()
            .WithEvent<EventEnvelopeRetrieved>()
            .WithEvent<HandleEvent>()
            .WithEvent<EventHandled>()
            .WithEvent<AcknowledgeEvent>()
            .WithEvent<EventAcknowledged>();

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