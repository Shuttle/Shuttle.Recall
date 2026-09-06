using Shuttle.Access.AspNetCore;
using Shuttle.Contract;

namespace Shuttle.Recall.WebApi;

public static class EventStoreContextExtensions
{
    extension(IEventStoreContext eventStoreContext)
    {
        public bool HasAccess(ISessionContext sessionContext)
        {
            var permission = Guard.AgainstNull(eventStoreContext).EventStore.Permission;

            return string.IsNullOrWhiteSpace(permission) || Guard.AgainstNull(sessionContext).HasPermission(permission);
        }

        public string? ValidateEventStore()
        {
            var eventStore = Guard.AgainstNull(eventStoreContext).EventStore;

            if (string.IsNullOrWhiteSpace(eventStore.ConnectionString))
            {
                return $"No connection string has been configured for the '{eventStore.Name}' event store.";
            }

            return string.IsNullOrWhiteSpace(eventStore.Schema)
                ? $"No schema has been configured for the '{eventStore.Name}' event store."
                : null;
        }
    }
}
