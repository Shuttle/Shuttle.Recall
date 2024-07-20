using System;
using System.Collections.Generic;
using System.Linq;
using Shuttle.Core.Contract;

namespace Shuttle.Recall
{
    public class ProjectionConfiguration : IProjectionConfiguration
    {
        private readonly Dictionary<string, List<Type>> _projectionNameEventHandlerTypes = new Dictionary<string, List<Type>>();

        public void AddProjectionEventHandlerType(string projectionName, Type eventHandlerType)
        {
            Guard.AgainstNullOrEmptyString(projectionName, nameof(projectionName));
            Guard.AgainstNull(eventHandlerType, nameof(eventHandlerType));

            if (!_projectionNameEventHandlerTypes.ContainsKey(projectionName))
            {
                _projectionNameEventHandlerTypes.Add(projectionName, new List<Type>());
            }

            _projectionNameEventHandlerTypes[projectionName].Add(eventHandlerType);
        }

        public IEnumerable<string> GetProjectionNames()
        {
            return _projectionNameEventHandlerTypes.Keys;
        }

        public IEnumerable<Type> GetEventHandlerTypes(string projectionName)
        {
            Guard.AgainstNullOrEmptyString(projectionName, nameof(projectionName));

            return _projectionNameEventHandlerTypes[projectionName] ?? Enumerable.Empty<Type>();
        }
    }
}