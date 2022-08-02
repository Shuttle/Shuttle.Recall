using System;
using System.Collections;
using System.Collections.Generic;

namespace Shuttle.Recall
{
    public interface IEventStoreConfiguration
    {
        IEnumerable<string> GetProjectionNames();
        IEnumerable<Type> GetEventHandlerTypes(string projectionName);
    }
}