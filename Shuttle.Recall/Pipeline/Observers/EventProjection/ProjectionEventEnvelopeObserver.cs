using System.Threading.Tasks;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Recall;

public interface IProjectionEventEnvelopeObserver : IPipelineObserver<OnGetProjectionEventEnvelope>
{
}

public class ProjectionEventEnvelopeObserver : IProjectionEventEnvelopeObserver
{
    private readonly IPipelineFactory _pipelineFactory;

    public ProjectionEventEnvelopeObserver(IPipelineFactory pipelineFactory)
    {
        _pipelineFactory = Guard.AgainstNull(pipelineFactory);
    }

    public async Task ExecuteAsync(IPipelineContext<OnGetProjectionEventEnvelope> pipelineContext)
    {
        var state = Guard.AgainstNull(pipelineContext).Pipeline.State;
        var projectionEvent = state.GetProjectionEvent();

        if (!projectionEvent.HasPrimitiveEvent)
        {
            return;
        }

        var pipeline = _pipelineFactory.GetPipeline<GetEventEnvelopePipeline>();

        try
        {
            await pipeline.ExecuteAsync(projectionEvent.PrimitiveEvent);

            state.SetEventEnvelope(pipeline.State.GetEventEnvelope());
            state.SetEvent(pipeline.State.GetEvent());
        }
        finally
        {
            _pipelineFactory.ReleasePipeline(pipeline);
        }
    }
}