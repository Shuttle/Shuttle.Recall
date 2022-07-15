using System.Linq;
using Shuttle.Core.Contract;

namespace Shuttle.Recall
{
    public static class EventStoreOptionsExtensions
    {
        public static bool HasActiveProjection(this EventStoreOptions eventStoreOptions, string name)
        {
            Guard.AgainstNull(eventStoreOptions, nameof(eventStoreOptions));

            if (string.IsNullOrEmpty(name))
            {
                return false;
            }

            if (eventStoreOptions.ActiveProjections.Count == 1)
            {
                var value = eventStoreOptions.ActiveProjections.ElementAt(0);

                if (value.Equals("!"))
                {
                    return false;
                }

                if (value.Equals("*"))
                {
                    return true;
                }
            }

            return !eventStoreOptions.ActiveProjections.Any() ||
                   eventStoreOptions.ActiveProjections.FirstOrDefault(item => item.Equals(name)) != null;
        }
    }
}