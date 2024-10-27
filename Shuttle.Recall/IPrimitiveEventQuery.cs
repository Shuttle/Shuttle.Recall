using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shuttle.Recall;

public interface IPrimitiveEventQuery
{
    Task<IEnumerable<PrimitiveEvent>> SearchAsync(PrimitiveEvent.Specification specification);
}