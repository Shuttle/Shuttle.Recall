using Shuttle.Core.Contract;
using Shuttle.Core.Threading;

namespace Shuttle.Recall;

public class PrimitiveEventSequencerProcessor(IPrimitiveEventSequencer primitiveEventSequencer, IThreadActivity threadActivity) : IProcessor
{
    private readonly IPrimitiveEventSequencer _primitiveEventSequencer = Guard.AgainstNull(primitiveEventSequencer);
    private readonly IThreadActivity _threadActivity = Guard.AgainstNull(threadActivity);

    public async Task ExecuteAsync(IProcessorThreadContext processorThread, CancellationToken cancellationToken = default)
    {
        if (await _primitiveEventSequencer.SequenceAsync(cancellationToken))
        {
            _threadActivity.Working();
        }
        else
        {
            await _threadActivity.WaitingAsync(cancellationToken);
        }
    }
}