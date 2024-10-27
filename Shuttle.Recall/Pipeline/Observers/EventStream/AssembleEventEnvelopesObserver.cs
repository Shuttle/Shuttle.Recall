using System.Collections.Generic;
using System.Threading.Tasks;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Recall;

public interface IAssembleEventEnvelopesObserver : IPipelineObserver<OnAssembleEventEnvelopes>
{
}

public class AssembleEventEnvelopesObserver : IAssembleEventEnvelopesObserver
{
    private readonly IPipelineFactory _pipelineFactory;

    public AssembleEventEnvelopesObserver(IPipelineFactory pipelineFactory)
    {
        _pipelineFactory = Guard.AgainstNull(pipelineFactory);
    }

    public async Task ExecuteAsync(IPipelineContext<OnAssembleEventEnvelopes> pipelineContext)
    {
        var state = Guard.AgainstNull(pipelineContext).Pipeline.State;
        var eventStream = Guard.AgainstNull(state.GetEventStream());
        var builder = Guard.AgainstNull(state.GetEventStreamBuilder());
        var eventEnvelopes = new List<EventEnvelope>();

        var pipeline = _pipelineFactory.GetPipeline<AssembleEventEnvelopePipeline>();

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
            _pipelineFactory.ReleasePipeline(pipeline);
        }

        state.SetEventEnvelopes(eventEnvelopes);
    }
}