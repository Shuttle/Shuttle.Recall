using System;
using System.Collections.Generic;
using System.Linq;
using Shuttle.Core.Contract;

namespace Shuttle.Recall;

public class ProjectionConfiguration : IProjectionConfiguration
{
    private readonly Dictionary<string, List<Type>> _projectionNameEventHandlerTypes = new();

    public void AddProjectionEventHandlerType(string projectionName, Type eventHandlerType)
    {
        Guard.AgainstNullOrEmptyString(projectionName);
        Guard.AgainstNull(eventHandlerType);

        if (!_projectionNameEventHandlerTypes.ContainsKey(projectionName))
        {
            _projectionNameEventHandlerTypes.Add(projectionName, new());
        }

        _projectionNameEventHandlerTypes[projectionName].Add(eventHandlerType);
    }

    public IEnumerable<string> GetProjectionNames()
    {
        return _projectionNameEventHandlerTypes.Keys;
    }

    public IEnumerable<Type> GetEventHandlerTypes(string projectionName)
    {
        return _projectionNameEventHandlerTypes[Guard.AgainstNullOrEmptyString(projectionName)];
    }
}