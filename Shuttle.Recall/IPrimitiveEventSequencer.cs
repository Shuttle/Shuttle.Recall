namespace Shuttle.Recall;

/// <summary>
/// Implementation should sequence PrimitiveEvents where the `SequenceNumber` is `null`.
/// It is possible that multiple instances may be run from multiple workers, which requires a locking strategy when updating the sequence numbers.
/// The implementation should be registered as a singleton.
/// </summary>
public interface IPrimitiveEventSequencer
{
    ValueTask<bool> SequenceAsync(CancellationToken cancellationToken = default);
}