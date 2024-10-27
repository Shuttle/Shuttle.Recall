using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shuttle.Recall;

public class NotImplementedPrimitiveEventRepository : IPrimitiveEventRepository
{
    public void Remove(Guid id)
    {
        throw new NotImplementedException(Resources.NotImplementedPrimitiveEventRepository);
    }

    public IEnumerable<PrimitiveEvent> Get(Guid id)
    {
        throw new NotImplementedException(Resources.NotImplementedPrimitiveEventRepository);
    }

    public long Save(PrimitiveEvent primitiveEvent)
    {
        throw new NotImplementedException(Resources.NotImplementedPrimitiveEventRepository);
    }

    public long GetSequenceNumber(Guid id)
    {
        throw new NotImplementedException(Resources.NotImplementedPrimitiveEventRepository);
    }

    public Task RemoveAsync(Guid id)
    {
        throw new NotImplementedException(Resources.NotImplementedPrimitiveEventRepository);
    }

    public Task<IEnumerable<PrimitiveEvent>> GetAsync(Guid id)
    {
        throw new NotImplementedException(Resources.NotImplementedPrimitiveEventRepository);
    }

    public ValueTask<long> SaveAsync(PrimitiveEvent primitiveEvent)
    {
        throw new NotImplementedException(Resources.NotImplementedPrimitiveEventRepository);
    }

    public ValueTask<long> GetSequenceNumberAsync(Guid id)
    {
        throw new NotImplementedException(Resources.NotImplementedPrimitiveEventRepository);
    }
}