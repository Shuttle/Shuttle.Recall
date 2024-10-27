using System.Threading;
using System.Threading.Tasks;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Threading;

namespace Shuttle.Recall;

public class ProjectionProcessor : IProcessor
{
    private readonly IEventProcessor _eventProcessor;
    private readonly IPipelineFactory _pipelineFactory;
    private readonly IThreadActivity _threadActivity;

    public ProjectionProcessor(EventStoreOptions eventStoreOptions, IPipelineFactory pipelineFactory, IEventProcessor eventProcessor)
    {
        _threadActivity = new ThreadActivity(Guard.AgainstNull(eventStoreOptions).DurationToSleepWhenIdle);
        _pipelineFactory = Guard.AgainstNull(pipelineFactory);
        _eventProcessor = Guard.AgainstNull(eventProcessor);
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken)
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

                await pipeline.ExecuteAsync(cancellationToken).ConfigureAwait(false);

                if (pipeline.State.GetWorking())
                {
                    _threadActivity.Working();
                    waiting = false;
                }

                _eventProcessor.ReleaseProjection(projection);
            }

            if (waiting)
            {
                await _threadActivity.WaitingAsync(cancellationToken);
            }
        }

        _pipelineFactory.ReleasePipeline(pipeline);
    }
}