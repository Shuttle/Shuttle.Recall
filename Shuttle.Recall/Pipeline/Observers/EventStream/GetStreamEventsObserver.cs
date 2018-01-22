using System;
using System.Collections.Generic;
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
            var state = pipelineEvent.Pipeline.State;
            var events = new List<object>();
            var pipeline = _pipelineFactory.GetPipeline<GetEventEnvelopePipeline>();

            try
            {
                var version = 0;

                foreach (var primitiveEvent in _primitiveEventRepository.Get(state.GetId()))
                {
                    if (primitiveEvent.Version < version)
                    {
                        throw new InvalidOperationException(string.Format(Resources.InvalidEventOrderingException,
                            primitiveEvent.Version, version));
                    }

                    pipeline.Execute(primitiveEvent);

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