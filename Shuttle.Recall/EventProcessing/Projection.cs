using Shuttle.Contract;

namespace Shuttle.Recall;

public class Projection(string name, long sequenceNumber, int failureCount = 0, DateTimeOffset? deferredUntil = null)
{
    public string Name { get; } = Guard.AgainstEmpty(name);
    public long SequenceNumber { get; private set; } = sequenceNumber;
    public int FailureCount { get; private set; } = failureCount;
    public DateTimeOffset? DeferredUntil { get; private set; } = deferredUntil;

    public Projection Commit(long sequenceNumber)
    {
        if (sequenceNumber >= SequenceNumber)
        {
            SequenceNumber = sequenceNumber;
        }

        return this;
    }

    public Projection Failed()
    {
        FailureCount++;
        return this;
    }

    public Projection Defer(DateTimeOffset? deferredUntil)
    {
        DeferredUntil = deferredUntil;
        return this;
    }
}