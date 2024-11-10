using Shuttle.Core.Contract;

namespace Shuttle.Recall;

public static class ProjectionBuilderExtensions
{
    public static ProjectionBuilder AddEventHandler<TEventHandler>(this ProjectionBuilder projectionBuilder) where TEventHandler : class
    {
        Guard.AgainstNull(projectionBuilder).AddEventHandler(typeof(TEventHandler));

        return projectionBuilder;
    }
}