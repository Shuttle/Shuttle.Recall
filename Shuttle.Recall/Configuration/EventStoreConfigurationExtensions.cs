using System.Linq;
using Shuttle.Core.Contract;

namespace Shuttle.Recall
{
    public static class EventStoreConfigurationExtensions
    {
        public static bool HasActiveProjection(this IEventStoreConfiguration configuration, string name)
        {
            Guard.AgainstNull(configuration, nameof(configuration));

            if (string.IsNullOrEmpty(name))
            {
                return false;
            }

            return !configuration.ActiveProjectionNames.Any() ||
                   configuration.ActiveProjectionNames.FirstOrDefault(item => item.Equals(name)) != null;
        }
    }
}