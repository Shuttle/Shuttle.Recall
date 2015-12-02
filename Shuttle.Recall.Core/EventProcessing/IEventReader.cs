using System;
using System.Collections.Generic;

namespace Shuttle.Recall.Core
{
    public interface IEventReader
    {
        EventRead GetEvent(long sequenceNumber);
        EventRead GetEvent(long sequenceNumber, IEnumerable<Type> eventTypes);
    }
}