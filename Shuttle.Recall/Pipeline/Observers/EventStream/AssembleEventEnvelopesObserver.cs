using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Recall;

public interface IAssembleEventEnvelopesObserver : IPipelineObserver<AssembleEventEnvelopes>;

public class AssembleEventEnvelopesObserver(IPipelineFactory pipelineFactory) : IAssembleEventEnvelopesObserver
{
    private readonly IPipelineFactory _pipelineFactory = Guard.AgainstNull(pipelineFactory);

    public async Task ExecuteAsync(IPipelineContext<AssembleEventEnvelopes> pipelineContext, CancellationToken cancellationToken = default)
    {
        var state = Guard.AgainstNull(pipelineContext).Pipeline.State;
        var eventStream = state.GetEventStream();
        var builder = state.GetEventStreamBuilder();
        var eventEnvelopes = new List<EventEnvelope>();

        var pipeline = await _pipelineFactory.GetPipelineAsync<AssembleEventEnvelopePipeline>(cancellationToken);

        pipeline.State.SetEventStreamBuilder(builder);

        try
        {
            foreach (var appendedEvent in eventStream.GetEvents())
            {
                eventEnvelopes.Add(await pipeline.ExecuteAsync(appendedEvent).ConfigureAwait(false));
            }

            state.SetEventEnvelopes(eventEnvelopes);
        }
        finally
        {
            await _pipelineFactory.ReleasePipelineAsync(pipeline, cancellationToken);
        }

        state.SetEventEnvelopes(eventEnvelopes);
    }
}