using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Threading;

namespace Shuttle.Recall;

public class ProjectionProcessor(IPipelineFactory pipelineFactory, IProcessorContext processorContext) : IProcessor
{
    private readonly IPipelineFactory _pipelineFactory = Guard.AgainstNull(pipelineFactory);
    private readonly IProcessorContext _processorContext = Guard.AgainstNull(processorContext);

    public async ValueTask<bool> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var pipeline = await _pipelineFactory.GetPipelineAsync<EventProcessingPipeline>(cancellationToken);

        pipeline.State.Clear();
        pipeline.State.SetProcessorThreadManagedThreadId(_processorContext.ManagedThreadId);

        await pipeline.ExecuteAsync(cancellationToken).ConfigureAwait(false);

        return !pipeline.Aborted && pipeline.State.GetWorkPerformed();
    }
}