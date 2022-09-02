using System;
using System.Collections.Generic;

namespace Shuttle.Recall
{
    public interface IPrimitiveEventQuery
    {
        IEnumerable<PrimitiveEvent> Search(PrimitiveEvent.Specification specification);
    }
}