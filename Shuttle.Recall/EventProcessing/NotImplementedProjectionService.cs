using System;
using System.Threading.Tasks;

namespace Shuttle.Recall;

public class NotImplementedProjectionService : IProjectionService
{
    public Task<ProjectionEvent?> GetProjectionEventAsync()
    {
        throw new NotImplementedException(Resources.NotImplementedProjectionEventProvider);
    }

    public Task SetSequenceNumberAsync(string projectionName, long sequenceNumber)
    {
        throw new NotImplementedException(Resources.NotImplementedProjectionEventProvider);
    }

    public ValueTask<long> GetSequenceNumberAsync(string projectionName)
    {
        throw new NotImplementedException(Resources.NotImplementedProjectionEventProvider);
    }
}