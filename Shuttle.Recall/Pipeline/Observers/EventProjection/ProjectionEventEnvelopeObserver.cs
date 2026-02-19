using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Recall;

public interface IProjectionEventEnvelopeObserver : IPipelineObserver<RetrieveEventEnvelope>;

public class ProjectionEventEnvelopeObserver(IGetEventEnvelopePipeline getEventEnvelopePipeline) : IProjectionEventEnvelopeObserver
{
    public async Task ExecuteAsync(IPipelineContext<RetrieveEventEnvelope> pipelineContext, CancellationToken cancellationToken = default)
    {
        var state = Guard.AgainstNull(pipelineContext).Pipeline.State;
        var projectionEvent = Guard.AgainstNull(state.GetProjectionEvent());

        await Guard.AgainstNull(getEventEnvelopePipeline).ExecuteAsync(projectionEvent.PrimitiveEvent);

        state.SetEventEnvelope(getEventEnvelopePipeline.State.GetEventEnvelope());
        state.SetDomainEvent(getEventEnvelopePipeline.State.GetDomainEvent());
    }
}