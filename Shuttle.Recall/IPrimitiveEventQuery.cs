using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shuttle.Recall
{
    public interface IPrimitiveEventQuery
    {
        IEnumerable<PrimitiveEvent> Search(PrimitiveEvent.Specification specification);
        Task<IEnumerable<PrimitiveEvent>> SearchAsync(PrimitiveEvent.Specification specification);
    }
}