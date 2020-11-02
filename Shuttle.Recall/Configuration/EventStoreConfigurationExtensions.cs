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

            if (configuration.ActiveProjectionNames.Count() == 1)
            {
                var value = configuration.ActiveProjectionNames.ElementAt(0);

                if (value.Equals("!"))
                {
                    return false;
                }

                if (value.Equals("*"))
                {
                    return true;
                }
            }

            return !configuration.ActiveProjectionNames.Any() ||
                   configuration.ActiveProjectionNames.FirstOrDefault(item => item.Equals(name)) != null;
        }
    }
}