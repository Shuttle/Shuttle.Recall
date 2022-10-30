using System.Collections.Generic;

namespace Shuttle.Recall
{
    public class NotImplementedPrimitiveEventQuery : IPrimitiveEventQuery
    {
        public IEnumerable<PrimitiveEvent> Search(PrimitiveEvent.Specification specification)
        {
            throw new System.NotImplementedException();
        }
    }
}