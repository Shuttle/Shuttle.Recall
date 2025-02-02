using System.Threading;
using System.Threading.Tasks;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Threading;

namespace Shuttle.Recall;

public class ProjectionProcessor : IProcessor
{
    private readonly IPipelineFactory _pipelineFactory;
    private readonly IThreadActivity _threadActivity;

    public ProjectionProcessor(EventStoreOptions eventStoreOptions, IPipelineFactory pipelineFactory)
    {
        _threadActivity = new ThreadActivity(Guard.AgainstNull(eventStoreOptions).DurationToSleepWhenIdle);
        _pipelineFactory = Guard.AgainstNull(pipelineFactory);
    }

    public async Task ExecuteAsync(IProcessorThreadContext context, CancellationToken cancellationToken = new())
    {
        var pipeline = _pipelineFactory.GetPipeline<EventProcessingPipeline>();

        while (!cancellationToken.IsCancellationRequested)
        {
            var waiting = true;

            pipeline.State.Clear();
            pipeline.State.SetProcessorThreadManagedThreadId((int)(context.State.Get("ManagedThreadId") ?? 0));

            await pipeline.ExecuteAsync(cancellationToken).ConfigureAwait(false);

            if (!pipeline.Aborted && pipeline.State.GetWorking())
            {
                _threadActivity.Working();
                waiting = false;
            }

            if (waiting)
            {
                await _threadActivity.WaitingAsync(cancellationToken);
            }
        }

        _pipelineFactory.ReleasePipeline(pipeline);
    }
}