using Shuttle.Core.Contract;
using Shuttle.Core.Threading;

namespace Shuttle.Recall;

public class PrimitiveEventSequencerProcessor(IPrimitiveEventSequencer primitiveEventSequencer) : IProcessor
{
    private readonly IPrimitiveEventSequencer _primitiveEventSequencer = Guard.AgainstNull(primitiveEventSequencer);

    public async ValueTask<bool> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        return await _primitiveEventSequencer.SequenceAsync(cancellationToken);
    }
}