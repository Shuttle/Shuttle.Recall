using Shuttle.Core.Contract;

namespace Shuttle.Recall;

public static class EventHandlerContextExtensions
{
    extension(IEventHandlerContext eventHandlerContext)
    {
        public bool IsDeferred()
        {
            return Guard.AgainstNull(eventHandlerContext).DeferredFor.HasValue;
        }
    }
}