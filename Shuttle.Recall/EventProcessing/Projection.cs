using System;
using System.Collections.Generic;
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

    public void Processed(long sequenceNumber)
    {
        if (sequenceNumber < SequenceNumber)
        {
            throw new InvalidOperationException(string.Format(Resources.ProjectionSequenceNumberException, Name, SequenceNumber, sequenceNumber));
        }

        SequenceNumber = sequenceNumber;
    }
}