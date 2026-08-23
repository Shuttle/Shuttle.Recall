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
    }
}
