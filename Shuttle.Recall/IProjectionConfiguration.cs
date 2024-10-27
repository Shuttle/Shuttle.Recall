using System;
using System.Collections.Generic;

namespace Shuttle.Recall;

public interface IProjectionConfiguration
{
    void AddProjectionEventHandlerType(string projectionName, Type eventHandlerType);
    IEnumerable<Type> GetEventHandlerTypes(string projectionName);
    IEnumerable<string> GetProjectionNames();
}