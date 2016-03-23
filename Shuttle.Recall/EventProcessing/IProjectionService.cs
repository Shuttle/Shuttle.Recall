using System;
using System.Collections.Generic;

namespace Shuttle.Recall
{
    public interface IProjectionService
    {
        long GetSequenceNumber(string name);
        void SetSequenceNumber(string name, long sequenceNumber);
		ProjectionEvent GetEvent(long sequenceNumber);
		ProjectionEvent GetEvent(long sequenceNumber, IEnumerable<Type> eventTypes);
	}
}