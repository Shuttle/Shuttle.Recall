using Microsoft.Extensions.DependencyInjection;
using Shuttle.Core.Contract;
using System;

namespace Shuttle.Recall;

public static class ProjectionBuilderExtensions
{
    public static ProjectionBuilder AddEventHandler<TEventHandler>(this ProjectionBuilder projectionBuilder, Func<Type, ServiceLifetime>? getServiceLifetime = null) where TEventHandler : class
    {
        Guard.AgainstNull(projectionBuilder).AddEventHandler(typeof(TEventHandler), getServiceLifetime);

        return projectionBuilder;
    }
}