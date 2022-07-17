using System.Collections.Generic;

namespace Shuttle.Recall
{
    public interface IEventStoreConfiguration
    {
        IEnumerable<string> GetProjectionNames();
    }
}