using Shuttle.Core.Container;
using Shuttle.Core.Contract;

namespace Shuttle.Recall
{
    public static class ComponentResolverExtensions
    {
        public static IComponentResolver AddEventHandler<TEventHandler>(this IComponentResolver resolver, string projectionName)
            where TEventHandler : class
        {
            Guard.AgainstNull(resolver, nameof(resolver));
            Guard.AgainstNullOrEmptyString(projectionName, nameof(projectionName));

            resolver
                .Resolve<IEventProcessor>()
                .GetProjection(projectionName)
                .AddEventHandler(resolver.Resolve<TEventHandler>());

            return resolver;
        }

        public static IComponentResolver AddEventHandler<TEventHandler>(this IComponentResolver resolver, Projection projection)
            where TEventHandler : class
        {
            Guard.AgainstNull(resolver, nameof(resolver));
            Guard.AgainstNull(projection, nameof(projection));

            projection.AddEventHandler(resolver.Resolve<TEventHandler>());

            return resolver;
        }
    }
}