using System;
using System.Collections.Generic;

namespace Shuttle.Recall.Core
{
    public interface IProjectionEventReader
    {
        ProjectionEvent GetEvent(long sequenceNumber);
        ProjectionEvent GetEvent(long sequenceNumber, IEnumerable<Type> eventTypes);
    }
}