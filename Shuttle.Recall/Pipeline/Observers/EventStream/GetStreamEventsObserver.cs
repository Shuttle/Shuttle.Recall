using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Recall
{
    public interface IGetStreamEventsObserver : IPipelineObserver<OnGetStreamEvents>
    {
    }

    public class GetStreamEventsObserver : IGetStreamEventsObserver
    {
        private readonly IPipelineFactory _pipelineFactory;
        private readonly IPrimitiveEventRepository _primitiveEventRepository;

        public GetStreamEventsObserver(IPipelineFactory pipelineFactory,
            IPrimitiveEventRepository primitiveEventRepository)
        {
            Guard.AgainstNull(pipelineFactory, nameof(pipelineFactory));
            Guard.AgainstNull(primitiveEventRepository, nameof(primitiveEventRepository));

            _pipelineFactory = pipelineFactory;
            _primitiveEventRepository = primitiveEventRepository;
        }

        public void Execute(OnGetStreamEvents pipelineEvent)
        {
            ExecuteAsync(pipelineEvent, true).GetAwaiter().GetResult();
        }

        public async Task ExecuteAsync(OnGetStreamEvents pipelineEvent)
        {
            await ExecuteAsync(pipelineEvent, false).ConfigureAwait(false);
        }

        public async Task ExecuteAsync(OnGetStreamEvents pipelineEvent, bool sync)
        {
            var state = Guard.AgainstNull(pipelineEvent, nameof(pipelineEvent)).Pipeline.State;
            var events = new List<DomainEvent>();
            var pipeline = _pipelineFactory.GetPipeline<GetEventEnvelopePipeline>();

            try
            {
                var version = 0;

                var primitiveEvents = sync
                    ? _primitiveEventRepository.Get(state.GetId())
                    : await _primitiveEventRepository.GetAsync(state.GetId()).ConfigureAwait(false);

                foreach (var primitiveEvent in primitiveEvents)
                {
                    if (primitiveEvent.Version < version)
                    {
                        throw new InvalidOperationException(string.Format(Resources.InvalidEventOrderingException,
                            primitiveEvent.Version, version));
                    }

                    if (sync)
                    {
                        pipeline.Execute(primitiveEvent);
                    }
                    else
                    {
                        await pipeline.ExecuteAsync(primitiveEvent).ConfigureAwait(false);
                    }

                    events.Add(pipeline.State.GetEvent());

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
}