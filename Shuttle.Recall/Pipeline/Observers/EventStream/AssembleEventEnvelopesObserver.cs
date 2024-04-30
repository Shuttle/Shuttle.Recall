using System.Collections.Generic;
using System.Threading.Tasks;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Recall
{
    public interface IAssembleEventEnvelopesObserver : IPipelineObserver<OnAssembleEventEnvelopes>
    {
    }

    public class AssembleEventEnvelopesObserver : IAssembleEventEnvelopesObserver
    {
        private readonly IPipelineFactory _pipelineFactory;

        public AssembleEventEnvelopesObserver(IPipelineFactory pipelineFactory)
        {
            Guard.AgainstNull(pipelineFactory, nameof(pipelineFactory));

            _pipelineFactory = pipelineFactory;
        }

        public void Execute(OnAssembleEventEnvelopes pipelineEvent)
        {
            ExecuteAsync(pipelineEvent, true).GetAwaiter().GetResult();
        }

        public async Task ExecuteAsync(OnAssembleEventEnvelopes pipelineEvent)
        {
            await ExecuteAsync(pipelineEvent, false).ConfigureAwait(false);
        }

        private async Task ExecuteAsync(OnAssembleEventEnvelopes pipelineEvent, bool sync)
        {
            var state = Guard.AgainstNull(pipelineEvent, nameof(pipelineEvent)).Pipeline.State;
            var eventStream = Guard.AgainstNull(state.GetEventStream(), StateKeys.EventStream);
            var configurator = Guard.AgainstNull(state.GetSaveEventStreamBuilder(), StateKeys.SaveEventStreamBuilder);
            var eventEnvelopes = new List<EventEnvelope>();

            var pipeline = _pipelineFactory.GetPipeline<AssembleEventEnvelopePipeline>();

            pipeline.State.SetSaveEventStreamBuilder(configurator);

            try
            {
                foreach (var appendedEvent in eventStream.GetEvents())
                {
                    if (sync)
                    {
                        eventEnvelopes.Add(pipeline.Execute(appendedEvent));
                    }
                    else
                    {
                        eventEnvelopes.Add(await pipeline.ExecuteAsync(appendedEvent).ConfigureAwait(false));
                    }
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