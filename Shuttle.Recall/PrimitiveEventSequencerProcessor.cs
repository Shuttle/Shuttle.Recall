using Microsoft.Extensions.Logging;
using Shuttle.Core.Contract;
using Shuttle.Core.Threading;

namespace Shuttle.Recall;

public class PrimitiveEventSequencerProcessor(IPrimitiveEventSequencer primitiveEventSequencer, ILogger<PrimitiveEventSequencerProcessor> logger) : IProcessor
{
    private readonly IPrimitiveEventSequencer _primitiveEventSequencer = Guard.AgainstNull(primitiveEventSequencer);
    private readonly ILogger<PrimitiveEventSequencerProcessor> _logger = Guard.AgainstNull(logger);

    public async ValueTask<bool> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        LogMessage.PrimitiveEventSequencerProcessorExecute(_logger);

        return await _primitiveEventSequencer.SequenceAsync(cancellationToken);
    }
}