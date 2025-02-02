using System;
using Microsoft.Extensions.DependencyInjection;
using Shuttle.Core.Contract;
using Shuttle.Core.Reflection;

namespace Shuttle.Recall;

public class ProjectionBuilder
{
    private static readonly Type EventHandlerType = typeof(IEventHandler<>);
    private readonly IEventProcessorConfiguration _eventProcessorConfiguration;

    public string Name { get; }

    public ProjectionBuilder(IServiceCollection services, IEventProcessorConfiguration eventProcessorConfiguration, string name)
    {
        Services = Guard.AgainstNull(services);
        _eventProcessorConfiguration = Guard.AgainstNull(eventProcessorConfiguration);
        Name = Guard.AgainstNullOrEmptyString(name);
    }

    public IServiceCollection Services { get; }

    public ProjectionBuilder AddEventHandler(object handler)
    {
        var typesAddedCount = 0;
        var type = Guard.AgainstNull(handler).GetType();

        foreach (var interfaceType in type.InterfacesCastableTo(EventHandlerType))
        {
            var eventType = interfaceType.GetGenericArguments()[0];

            var serviceKey = $"[Shuttle.Recall.Projection/{Name}]:{Guard.AgainstNullOrEmptyString(eventType.FullName)}";

            if (Services.Contains(ServiceDescriptor.KeyedTransient(interfaceType, serviceKey, type)))
            {
                throw new InvalidOperationException(string.Format(Resources.DuplicateProjectionEventHandlerServiceException, interfaceType.FullName, Name, serviceKey));
            }

            Services.AddKeyedSingleton(interfaceType, serviceKey, handler);

            _eventProcessorConfiguration.GetProjection(Name).AddHandlerEventType(eventType);

            typesAddedCount++;
        }

        if (typesAddedCount == 0)
        {
            throw new EventProcessingException(string.Format(Resources.InvalidEventHandlerTypeExpection, type.FullName ?? type.Name));
        }

        return this;
    }

    public ProjectionBuilder AddEventHandler(Type type, Func<Type, ServiceLifetime>? getServiceLifetime = null)
    {
        getServiceLifetime ??= _ => ServiceLifetime.Singleton;

        var typesAddedCount = 0;

        foreach (var interfaceType in type.InterfacesCastableTo(EventHandlerType))
        {
            var eventType = interfaceType.GetGenericArguments()[0];

            var serviceKey = $"[Shuttle.Recall.Projection/{Name}]:{Guard.AgainstNullOrEmptyString(eventType.FullName)}";

            if (Services.Contains(ServiceDescriptor.KeyedTransient(interfaceType, serviceKey, type)))
            {
                throw new InvalidOperationException(string.Format(Resources.DuplicateProjectionEventHandlerServiceException, interfaceType.FullName, Name, serviceKey));
            }

            Services.Add(new(interfaceType, serviceKey, type, getServiceLifetime(interfaceType)));

            _eventProcessorConfiguration.GetProjection(Name).AddHandlerEventType(eventType);

            typesAddedCount++;
        }

        if (typesAddedCount == 0)
        {
            throw new EventProcessingException(string.Format(Resources.InvalidEventHandlerTypeExpection, type.FullName ?? type.Name));
        }

        return this;
    }

    public ProjectionBuilder AddEventHandler(Delegate handler)
    {
        _eventProcessorConfiguration.GetProjection(Name).AddEventHandler(handler);

        return this;
    }
}