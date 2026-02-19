using Microsoft.Extensions.Logging;
using Shuttle.Core.Contract;
using Shuttle.Core.Threading;

namespace Shuttle.Recall;

public class ProjectionProcessor(IEventProcessingPipeline eventProcessingPipeline, IProcessorContext processorContext, ILogger<ProjectionProcessor> logger) : IProcessor
{
    private readonly IProcessorContext _processorContext = Guard.AgainstNull(processorContext);
    private readonly ILogger<ProjectionProcessor> _logger = Guard.AgainstNull(logger);

    public async ValueTask<bool> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(eventProcessingPipeline);

        LogMessage.ProjectionProcessorExecute(_logger);

        eventProcessingPipeline.State.Clear();
        eventProcessingPipeline.State.SetProcessorThreadManagedThreadId(_processorContext.ManagedThreadId);

        await eventProcessingPipeline.ExecuteAsync(cancellationToken).ConfigureAwait(false);

        return !eventProcessingPipeline.Aborted && eventProcessingPipeline.State.GetWorkPerformed();
    }
}