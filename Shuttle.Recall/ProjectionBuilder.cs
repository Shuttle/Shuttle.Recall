using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shuttle.Core.Contract;
using Shuttle.Core.Reflection;

namespace Shuttle.Recall;

public class ProjectionBuilder
{
    private static readonly Type EventHandlerType = typeof(IEventHandler<>);
    private static readonly Type EventHandlerContextType = typeof(IEventHandlerContext<>);
    private readonly Dictionary<Type, List<ProjectionDelegate>> _delegates = new();
    private readonly Dictionary<Type, List<Type>> _eventHandlers= new();

    public string Name { get; }

    public ProjectionBuilder(string name)
    {
        Name = Guard.AgainstNullOrEmptyString(name);
    }

    public ProjectionBuilder AddEventHandler(Type type)
    {
        var typesAddedCount = 0;

        foreach (var interfaceType in type.InterfacesCastableTo(EventHandlerType))
        {
            var eventType = interfaceType.GetGenericArguments()[0];

            _eventHandlers.TryAdd(eventType, new());
            _eventHandlers[eventType].Add(type);

            typesAddedCount++;
        }

        if (typesAddedCount == 0)
        {
            throw new EventProcessingException(string.Format(Resources.InvalidEventHandlerTypeExpection, type.FullName ?? type.Name));
        }

        return this;
    }

    public ProjectionBuilder MapEventHandler(Delegate handler)
    {
        if (!typeof(Task).IsAssignableFrom(Guard.AgainstNull(handler).Method.ReturnType))
        {
            throw new ApplicationException(Resources.AsyncDelegateRequiredException);
        }

        var parameters = handler.Method.GetParameters();
        Type? messageType = null;

        foreach (var parameter in parameters)
        {
            var parameterType = parameter.ParameterType;

            if (parameterType.IsCastableTo(EventHandlerContextType))
            {
                messageType = parameterType.GetGenericArguments()[0];
            }
        }

        if (messageType == null)
        {
            throw new ApplicationException(Resources.EventHandlerTypeException);
        }

        _delegates.TryAdd(messageType, new());
        _delegates[messageType].Add(new(handler, handler.Method.GetParameters().Select(item => item.ParameterType)));

        return this;
    }
}

public static class ProjectionBuilderExtensions
{
    public static ProjectionBuilder AddEventHandler<TEventHandler>(this ProjectionBuilder projectionBuilder) where TEventHandler : class
    {
        Guard.AgainstNull(projectionBuilder).AddEventHandler(typeof(TEventHandler));

        return projectionBuilder;
    }
}