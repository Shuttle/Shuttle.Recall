using System;
using Shuttle.Core.Contract;

namespace Shuttle.Recall;

public class Projection
{
    public Projection(string name, long sequenceNumber)
    {
        Name = Guard.AgainstNullOrEmptyString(name);
        SequenceNumber = sequenceNumber;
    }

    public string Name { get; }
    public long SequenceNumber { get; private set; }

    public void Commit(long sequenceNumber)
    {
        if (sequenceNumber < SequenceNumber)
        {
            return;
        }

        SequenceNumber = sequenceNumber;
    }
}