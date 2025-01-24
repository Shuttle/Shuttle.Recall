using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shuttle.Recall;

public interface IPrimitiveEventRepository
{
    Task<IEnumerable<PrimitiveEvent>> GetAsync(Guid id);
    ValueTask<long> GetSequenceNumberAsync(Guid id);
    Task RemoveAsync(Guid id);
    ValueTask<long> SaveAsync(IEnumerable<PrimitiveEvent> primitiveEvents);
    ValueTask<long> GetMaxSequenceNumberAsync();
}