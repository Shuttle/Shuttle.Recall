using System;
using System.Threading.Tasks;

namespace Shuttle.Recall;

public class NotImplementedProjectionEventProvider : IProjectionEventProvider
{
    public Task<ProjectionEvent?> GetAsync()
    {
        throw new NotImplementedException(Resources.NotImplementedProjectionEventProvider);
    }
}