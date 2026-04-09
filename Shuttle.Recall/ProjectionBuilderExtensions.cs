using Microsoft.Extensions.DependencyInjection;
using Shuttle.Contract;

namespace Shuttle.Recall;

public static class ProjectionBuilderExtensions
{
    extension(ProjectionBuilder projectionBuilder)
    {
        public ProjectionBuilder AddEventHandler<TEventHandler>(Func<Type, ServiceLifetime>? getServiceLifetime = null) where TEventHandler : class
        {
            Guard.AgainstNull(projectionBuilder).AddEventHandler(typeof(TEventHandler), getServiceLifetime);

            return projectionBuilder;
        }
    }
}