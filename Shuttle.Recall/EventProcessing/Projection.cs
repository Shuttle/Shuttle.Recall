using Shuttle.Core.Contract;

namespace Shuttle.Recall;

public class Projection(string name, long sequenceNumber)
{
    public string Name { get; } = Guard.AgainstEmpty(name);
    public long SequenceNumber { get; private set; } = sequenceNumber;

    public void Commit(long sequenceNumber)
    {
        if (sequenceNumber < SequenceNumber)
        {
            return;
        }

        SequenceNumber = sequenceNumber;
    }
}