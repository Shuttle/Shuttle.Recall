using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shuttle.Recall;

public class NotImplementedPrimitiveEventQuery : IPrimitiveEventQuery
{
    public Task<IEnumerable<PrimitiveEvent>> SearchAsync(PrimitiveEvent.Specification specification)
    {
        throw new NotImplementedException(Resources.NotImplementedPrimitiveEventQuery);
    }
}