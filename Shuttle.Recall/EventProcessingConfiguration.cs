using System.Collections.Generic;
using System.Linq;
using Shuttle.Core.Contract;

namespace Shuttle.Recall;

public class EventProcessorConfiguration : IEventProcessorConfiguration
{
    private readonly Dictionary<string, ProjectionConfiguration> _projectionConfigurations = new();
    
    public ProjectionConfiguration GetProjection(string name)
    {
        if (!_projectionConfigurations.ContainsKey(Guard.AgainstNullOrEmptyString(name)))
        {
            _projectionConfigurations.Add(name, new(name));
        }

        return _projectionConfigurations[name];
    }

    public bool HasProjections => _projectionConfigurations.Any(projectionConfiguration => projectionConfiguration.Value.EventTypes.Any());
}