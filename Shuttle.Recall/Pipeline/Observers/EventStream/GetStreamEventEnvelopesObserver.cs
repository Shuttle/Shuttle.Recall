using System.Collections.Generic;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Recall
{
    public class GetStreamEventEnvelopesObserver : IPipelineObserver<OnGetStreamEventEnvelopes>
    {
        private readonly IPipelineFactory _pipelineFactory;
        private readonly IPrimitiveEventRepository _primitiveEventRepository;

        public GetStreamEventEnvelopesObserver(IPipelineFactory pipelineFactory, IPrimitiveEventRepository primitiveEventRepository)
        {
            Guard.AgainstNull(pipelineFactory, "pipelineFactory");
            Guard.AgainstNull(primitiveEventRepository, "primitiveEventRepository");

            _pipelineFactory = pipelineFactory;
            _primitiveEventRepository = primitiveEventRepository;
        }

        public void Execute(OnGetStreamEventEnvelopes pipelineEvent)
        {
            var state = pipelineEvent.Pipeline.State;
            var eventEnvelopes = new List<EventEnvelope>();
            var pipeline = _pipelineFactory.GetPipeline<GetEventEnvelopePipeline>();

            try
            {
                foreach (var primitiveEvent in _primitiveEventRepository.Get(state.GetId()))
                {
                    eventEnvelopes.Add(pipeline.Execute(primitiveEvent));
                }

                state.SetEventEnvelopes(eventEnvelopes);
            }
            finally
            {
                _pipelineFactory.ReleasePipeline(pipeline);
            }
        }
    }
}