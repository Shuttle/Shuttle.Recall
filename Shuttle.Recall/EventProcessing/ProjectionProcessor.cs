using System.Threading;
using System.Threading.Tasks;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Threading;

namespace Shuttle.Recall
{
    public class ProjectionProcessor : IProcessor
    {
        private readonly IEventProcessor _eventProcessor;
        private readonly IPipelineFactory _pipelineFactory;
        private readonly IThreadActivity _threadActivity;

        public ProjectionProcessor(EventStoreOptions eventStoreOptions, IPipelineFactory pipelineFactory, IEventProcessor eventProcessor)
        {
            _pipelineFactory = Guard.AgainstNull(pipelineFactory, nameof(pipelineFactory));
            _eventProcessor = Guard.AgainstNull(eventProcessor, nameof(eventProcessor));
            _threadActivity = new ThreadActivity(Guard.AgainstNull(eventStoreOptions, nameof(eventStoreOptions)).DurationToSleepWhenIdle);
        }

        public void Execute(CancellationToken cancellationToken)
        {
            ExecuteAsync(cancellationToken, true).GetAwaiter().GetResult();
        }

        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            await ExecuteAsync(cancellationToken, false).ConfigureAwait(false);
        }

        private async Task ExecuteAsync(CancellationToken cancellationToken, bool sync)
        {
            var pipeline = _pipelineFactory.GetPipeline<EventProcessingPipeline>();

            while (!cancellationToken.IsCancellationRequested)
            {
                var projection = _eventProcessor.GetProjection();
                var waiting = true;

                if (projection != null)
                {
                    pipeline.State.Clear();
                    pipeline.State.SetProjection(projection);

                    if (sync)
                    {
                        pipeline.Execute(cancellationToken);
                    }
                    else
                    {
                        await pipeline.ExecuteAsync(cancellationToken).ConfigureAwait(false);
                    }

                    if (pipeline.State.GetWorking())
                    {
                        _threadActivity.Working();
                        waiting = false;
                    }

                    _eventProcessor.ReleaseProjection(projection);
                }

                if (waiting)
                {
                    _threadActivity.Waiting(cancellationToken);
                }
            }

            _pipelineFactory.ReleasePipeline(pipeline);
        }
    }
}