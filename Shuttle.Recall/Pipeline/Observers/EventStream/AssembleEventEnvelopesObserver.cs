using Shuttle.Contract;
using Shuttle.Pipelines;

namespace Shuttle.Recall;

public interface IAssembleEventEnvelopesObserver : IPipelineObserver<AssembleEventEnvelopes>;

public class AssembleEventEnvelopesObserver(IAssembleEventEnvelopePipeline assembleEventEnvelopePipeline) : IAssembleEventEnvelopesObserver
{
    public async Task ExecuteAsync(IPipelineContext<AssembleEventEnvelopes> pipelineContext, CancellationToken cancellationToken = default)
    {
        var state = Guard.AgainstNull(pipelineContext).Pipeline.State;
        var eventStream = state.GetEventStream();
        var builder = state.GetEventStreamBuilder();
        var eventEnvelopes = new List<EventEnvelope>();

        Guard.AgainstNull(assembleEventEnvelopePipeline).State.SetEventStreamBuilder(builder);

        foreach (var appendedEvent in eventStream.GetEvents())
        {
            eventEnvelopes.Add(await assembleEventEnvelopePipeline.ExecuteAsync(appendedEvent).ConfigureAwait(false));
        }

        state.SetEventEnvelopes(eventEnvelopes);
    }
}