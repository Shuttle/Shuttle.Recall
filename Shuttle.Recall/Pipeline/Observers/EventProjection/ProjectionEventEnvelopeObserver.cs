using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Recall;

public interface IProjectionEventEnvelopeObserver : IPipelineObserver<RetrieveEventEnvelope>;

public class ProjectionEventEnvelopeObserver(IPipelineFactory pipelineFactory) : IProjectionEventEnvelopeObserver
{
    private readonly IPipelineFactory _pipelineFactory = Guard.AgainstNull(pipelineFactory);

    public async Task ExecuteAsync(IPipelineContext<RetrieveEventEnvelope> pipelineContext, CancellationToken cancellationToken = default)
    {
        var state = Guard.AgainstNull(pipelineContext).Pipeline.State;
        var projectionEvent = Guard.AgainstNull(state.GetProjectionEvent());

        var pipeline = await _pipelineFactory.GetPipelineAsync<GetEventEnvelopePipeline>(cancellationToken);

        await pipeline.ExecuteAsync(projectionEvent.PrimitiveEvent);

        state.SetEventEnvelope(pipeline.State.GetEventEnvelope());
        state.SetDomainEvent(pipeline.State.GetDomainEvent());
    }
}