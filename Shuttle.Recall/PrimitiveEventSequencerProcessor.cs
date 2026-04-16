using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Shuttle.Contract;
using Shuttle.Threading;

namespace Shuttle.Recall;

public class PrimitiveEventSequencerProcessor(IPrimitiveEventSequencer primitiveEventSequencer, ILogger<PrimitiveEventSequencerProcessor>? logger = null) : IProcessor
{
    private readonly IPrimitiveEventSequencer _primitiveEventSequencer = Guard.AgainstNull(primitiveEventSequencer);
    private readonly ILogger<PrimitiveEventSequencerProcessor> _logger = logger ?? NullLogger<PrimitiveEventSequencerProcessor>.Instance;

    public async ValueTask<bool> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        LogMessage.PrimitiveEventSequencerProcessorExecute(_logger);

        return await _primitiveEventSequencer.SequenceAsync(cancellationToken);
    }
}