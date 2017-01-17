using System;
using System.Collections.Generic;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Recall
{
    public class GetStreamEventsObserver : IPipelineObserver<OnGetStreamEvents>
    {
        private readonly IPipelineFactory _pipelineFactory;
        private readonly IPrimitiveEventRepository _primitiveEventRepository;

        public GetStreamEventsObserver(IPipelineFactory pipelineFactory, IPrimitiveEventRepository primitiveEventRepository)
        {
            Guard.AgainstNull(pipelineFactory, "pipelineFactory");
            Guard.AgainstNull(primitiveEventRepository, "primitiveEventRepository");

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
                        throw new InvalidOperationException(string.Format(RecallResources.InvalidEventOrderingException, primitiveEvent.Version, version));
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