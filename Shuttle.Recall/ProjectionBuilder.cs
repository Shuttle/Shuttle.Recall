using Microsoft.Extensions.DependencyInjection;
using Shuttle.Core.Contract;
using Shuttle.Core.Reflection;

namespace Shuttle.Recall;

public class ProjectionBuilder(IServiceCollection services, IEventProcessorConfiguration eventProcessorConfiguration, string name)
{
    private static readonly Type EventHandlerType = typeof(IEventHandler<>);
    private readonly IEventProcessorConfiguration _eventProcessorConfiguration = Guard.AgainstNull(eventProcessorConfiguration);

    public string Name { get; } = Guard.AgainstEmpty(name);

    public IServiceCollection Services { get; } = Guard.AgainstNull(services);

    public ProjectionBuilder AddEventHandler(object handler)
    {
        var typesAddedCount = 0;
        var type = Guard.AgainstNull(handler).GetType();

        foreach (var interfaceType in type.InterfacesCastableTo(EventHandlerType))
        {
            var eventType = interfaceType.GetGenericArguments()[0];

            var serviceKey = $"[Shuttle.Recall.Projection/{Name}]:{Guard.AgainstEmpty(eventType.FullName)}";

            if (Services.Contains(ServiceDescriptor.KeyedTransient(interfaceType, serviceKey, type)))
            {
                throw new InvalidOperationException(string.Format(Resources.DuplicateProjectionEventHandlerServiceException, interfaceType.FullName, Name, serviceKey));
            }

            Services.AddKeyedSingleton(interfaceType, serviceKey, handler);

            _eventProcessorConfiguration.GetProjection(Name).AddHandlerEventType(eventType);

            typesAddedCount++;
        }

        return typesAddedCount == 0 
            ? throw new EventProcessingException(string.Format(Resources.InvalidEventHandlerTypeExpection, type.FullName ?? type.Name)) 
            : this;
    }

    public ProjectionBuilder AddEventHandler(Type type, Func<Type, ServiceLifetime>? getServiceLifetime = null)
    {
        getServiceLifetime ??= _ => ServiceLifetime.Scoped;

        var typesAddedCount = 0;

        foreach (var interfaceType in type.InterfacesCastableTo(EventHandlerType))
        {
            var eventType = interfaceType.GetGenericArguments()[0];

            var serviceKey = $"[Shuttle.Recall.Projection/{Name}]:{Guard.AgainstEmpty(eventType.FullName)}";

            if (Services.Contains(ServiceDescriptor.KeyedTransient(interfaceType, serviceKey, type)))
            {
                throw new InvalidOperationException(string.Format(Resources.DuplicateProjectionEventHandlerServiceException, interfaceType.FullName, Name, serviceKey));
            }

            Services.Add(new(interfaceType, serviceKey, type, getServiceLifetime(interfaceType)));

            _eventProcessorConfiguration.GetProjection(Name).AddHandlerEventType(eventType);

            typesAddedCount++;
        }

        return typesAddedCount == 0 
            ? throw new EventProcessingException(string.Format(Resources.InvalidEventHandlerTypeExpection, type.FullName ?? type.Name)) 
            : this;
    }

    public ProjectionBuilder AddEventHandler(Delegate handler)
    {
        _eventProcessorConfiguration.GetProjection(Name).AddEventHandler(handler);

        return this;
    }
}