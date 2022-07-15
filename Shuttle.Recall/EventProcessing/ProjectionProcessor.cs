using System.Threading;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Threading;

namespace Shuttle.Recall
{
    public class ProjectionProcessor : IProcessor
    {
        private readonly IPipelineFactory _pipelineFactory;
        private readonly IEventProcessor _eventProcessor;
        private readonly IThreadActivity _threadActivity;

        public ProjectionProcessor(EventStoreOptions eventStoreOptions, IPipelineFactory pipelineFactory,
            IEventProcessor eventProcessor)
        {
            Guard.AgainstNull(eventStoreOptions, nameof(eventStoreOptions));
            Guard.AgainstNull(pipelineFactory, nameof(pipelineFactory));
            Guard.AgainstNull(eventProcessor, nameof(eventProcessor));

            _pipelineFactory = pipelineFactory;
            _eventProcessor = eventProcessor;
            _threadActivity = new ThreadActivity(eventStoreOptions.DurationToSleepWhenIdle);
        }

        public void Execute(CancellationToken cancellationToken)
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
                    pipeline.State.SetCancellationToken(cancellationToken);

                    pipeline.Execute();

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