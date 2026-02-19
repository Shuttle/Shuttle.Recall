using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Recall;

public interface IRetrieveStreamEventsObserver : IPipelineObserver<RetrieveStreamEvents>;

public class RetrieveStreamEventsObserver(IGetEventEnvelopePipeline getEventEnvelopePipeline, IPrimitiveEventRepository primitiveEventRepository)
    : IRetrieveStreamEventsObserver
{
    private readonly IPrimitiveEventRepository _primitiveEventRepository = Guard.AgainstNull(primitiveEventRepository);

    public async Task ExecuteAsync(IPipelineContext<RetrieveStreamEvents> pipelineContext, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(getEventEnvelopePipeline);

        var state = Guard.AgainstNull(pipelineContext).Pipeline.State;
        var events = new List<DomainEvent>();

        var version = 0;

        var primitiveEvents = await _primitiveEventRepository.GetAsync(state.GetId(), cancellationToken).ConfigureAwait(false);

        foreach (var primitiveEvent in primitiveEvents)
        {
            if (primitiveEvent.Version < version)
            {
                throw new InvalidOperationException(string.Format(Resources.InvalidEventOrderingException, primitiveEvent.Version, version));
            }

            await getEventEnvelopePipeline.ExecuteAsync(primitiveEvent).ConfigureAwait(false);

            events.Add(getEventEnvelopePipeline.State.GetDomainEvent());

            version = primitiveEvent.Version;
        }

        state.SetEvents(events);
        state.SetVersion(version);
    }
}