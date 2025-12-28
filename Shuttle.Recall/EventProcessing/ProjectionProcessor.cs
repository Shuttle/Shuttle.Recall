using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Threading;

namespace Shuttle.Recall;

public class ProjectionProcessor(EventStoreOptions eventStoreOptions, IPipelineFactory pipelineFactory)
    : IProcessor
{
    private readonly IPipelineFactory _pipelineFactory = Guard.AgainstNull(pipelineFactory);
    private readonly IThreadActivity _threadActivity = new ThreadActivity(Guard.AgainstNull(eventStoreOptions).ProjectionProcessorIdleDurations);

    public async Task ExecuteAsync(IProcessorThreadContext context, CancellationToken cancellationToken = default)
    {
        var pipeline = await _pipelineFactory.GetPipelineAsync<EventProcessingPipeline>(cancellationToken);

        while (!cancellationToken.IsCancellationRequested)
        {
            var waiting = true;
            var managedThreadId = (int)(context.State.Get("ManagedThreadId") ?? 0);

            pipeline.State.Clear();
            pipeline.State.SetProcessorThreadManagedThreadId(managedThreadId);

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

        await _pipelineFactory.ReleasePipelineAsync(pipeline, cancellationToken);
    }
}