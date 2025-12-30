using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Recall;

public interface IRetrieveStreamEventsObserver : IPipelineObserver<RetrieveStreamEvents>;

public class RetrieveStreamEventsObserver(IPipelineFactory pipelineFactory, IPrimitiveEventRepository primitiveEventRepository)
    : IRetrieveStreamEventsObserver
{
    private readonly IPipelineFactory _pipelineFactory = Guard.AgainstNull(pipelineFactory);
    private readonly IPrimitiveEventRepository _primitiveEventRepository = Guard.AgainstNull(primitiveEventRepository);

    public async Task ExecuteAsync(IPipelineContext<RetrieveStreamEvents> pipelineContext, CancellationToken cancellationToken = default)
    {
        var state = Guard.AgainstNull(pipelineContext).Pipeline.State;
        var events = new List<DomainEvent>();
        var pipeline = await _pipelineFactory.GetPipelineAsync<GetEventEnvelopePipeline>(cancellationToken);

        var version = 0;

        var primitiveEvents = await _primitiveEventRepository.GetAsync(state.GetId(), cancellationToken).ConfigureAwait(false);

        foreach (var primitiveEvent in primitiveEvents)
        {
            if (primitiveEvent.Version < version)
            {
                throw new InvalidOperationException(string.Format(Resources.InvalidEventOrderingException, primitiveEvent.Version, version));
            }

            await pipeline.ExecuteAsync(primitiveEvent).ConfigureAwait(false);

            events.Add(pipeline.State.GetDomainEvent());

            version = primitiveEvent.Version;
        }

        state.SetEvents(events);
        state.SetVersion(version);
    }
}