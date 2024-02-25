using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shuttle.Recall
{
    public class NotImplementedPrimitiveEventQuery : IPrimitiveEventQuery
    {
        public IEnumerable<PrimitiveEvent> Search(PrimitiveEvent.Specification specification)
        {
            throw new System.NotImplementedException();
        }

        public Task<IEnumerable<PrimitiveEvent>> SearchAsync(PrimitiveEvent.Specification specification)
        {
            throw new System.NotImplementedException();
        }
    }
}