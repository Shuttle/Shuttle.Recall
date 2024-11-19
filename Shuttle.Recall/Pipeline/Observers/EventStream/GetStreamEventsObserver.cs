using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Recall;

public interface IGetStreamEventsObserver : IPipelineObserver<OnGetStreamEvents>
{
}

public class GetStreamEventsObserver : IGetStreamEventsObserver
{
    private readonly IPipelineFactory _pipelineFactory;
    private readonly IPrimitiveEventRepository _primitiveEventRepository;

    public GetStreamEventsObserver(IPipelineFactory pipelineFactory, IPrimitiveEventRepository primitiveEventRepository)
    {
        _pipelineFactory = Guard.AgainstNull(pipelineFactory);
        _primitiveEventRepository = Guard.AgainstNull(primitiveEventRepository);
    }

    public async Task ExecuteAsync(IPipelineContext<OnGetStreamEvents> pipelineContext)
    {
        var state = Guard.AgainstNull(pipelineContext).Pipeline.State;
        var events = new List<DomainEvent>();
        var pipeline = _pipelineFactory.GetPipeline<GetEventEnvelopePipeline>();

        try
        {
            var version = 0;

            var primitiveEvents = await _primitiveEventRepository.GetAsync(state.GetId()).ConfigureAwait(false);

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
        finally
        {
            _pipelineFactory.ReleasePipeline(pipeline);
        }
    }
}