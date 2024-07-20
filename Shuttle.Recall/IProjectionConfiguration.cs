using System;
using System.Collections;
using System.Collections.Generic;

namespace Shuttle.Recall
{
    public interface IProjectionConfiguration
    {
        IEnumerable<string> GetProjectionNames();
        IEnumerable<Type> GetEventHandlerTypes(string projectionName);
        void AddProjectionEventHandlerType(string projectionName, Type eventHandlerType);
    }
}