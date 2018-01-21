using System.Collections.Generic;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Recall
{
    public class AssembleEventEnvelopesObserver : IPipelineObserver<OnAssembleEventEnvelopes>
    {
        private readonly IPipelineFactory _pipelineFactory;

        public AssembleEventEnvelopesObserver(IPipelineFactory pipelineFactory)
        {
            Guard.AgainstNull(pipelineFactory, nameof(pipelineFactory));

            _pipelineFactory = pipelineFactory;
        }

        public void Execute(OnAssembleEventEnvelopes pipelineEvent)
        {
            var state = pipelineEvent.Pipeline.State;
            var eventStream = state.GetEventStream();
            var configurator = state.GetEventEnvelopeConfigurator();
            var eventEnvelopes = new List<EventEnvelope>();

            Guard.AgainstNull(eventStream, nameof(eventStream));
            Guard.AgainstNull(configurator, nameof(configurator));

            var pipeline = _pipelineFactory.GetPipeline<AssembleEventEnvelopePipeline>();

            pipeline.State.SetEventEnvelopeConfigurator(configurator);

            try
            {
                foreach (var appendedEvent in eventStream.GetEvents())
                {
                    eventEnvelopes.Add(pipeline.Execute(appendedEvent));
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
}