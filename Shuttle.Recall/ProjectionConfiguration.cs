using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Shuttle.Core.Contract;
using Shuttle.Core.Reflection;

namespace Shuttle.Recall;

public class ProjectionConfiguration
{
    private static readonly Type EventHandlerContextType = typeof(IEventHandlerContext<>);
    private readonly Dictionary<Type, ProjectionDelegate> _delegates = new();
    private readonly List<Type> _handlerEventTypes = new();
    private readonly List<Type> _eventTypes = new();

    public IEnumerable<Type> EventTypes => _eventTypes;

    public ProjectionConfiguration(string name)
    {
        Name = Guard.AgainstNullOrEmptyString(name);
    }

    public string Name { get; }

    public void AddEventHandler(Delegate handler)
    {
        if (!typeof(Task).IsAssignableFrom(Guard.AgainstNull(handler).Method.ReturnType))
        {
            throw new ApplicationException(Resources.AsyncDelegateRequiredException);
        }

        var parameters = handler.Method.GetParameters();
        Type? eventType = null;

        foreach (var parameter in parameters)
        {
            var parameterType = parameter.ParameterType;

            if (parameterType.IsCastableTo(EventHandlerContextType))
            {
                eventType = parameterType.GetGenericArguments()[0];
            }
        }

        if (eventType == null)
        {
            throw new InvalidOperationException(Resources.EventHandlerTypeException);
        }

        if (_handlerEventTypes.Contains(eventType))
        {
            throw new InvalidOperationException(string.Format(Resources.ProjectionHandlerEventTypeAlreadyRegisteredException, eventType.FullName, Name));
        }

        if (!_delegates.TryAdd(eventType, new(handler, handler.Method.GetParameters().Select(item => item.ParameterType))))
        {
            throw new InvalidOperationException(string.Format(Resources.DuplicateProjectionDelegateException, eventType.FullName, Name));
        }

        _eventTypes.Add(eventType);
    }

    public void AddHandlerEventType(Type eventType)
    {
        if (_delegates.ContainsKey(Guard.AgainstNull(eventType)))
        {
            throw new InvalidOperationException(string.Format(Resources.ProjectionDelegateEventTypeAlreadyRegisteredException, eventType.FullName, Name));
        }

        if (_handlerEventTypes.Contains(eventType))
        {
            throw new InvalidOperationException(string.Format(Resources.DuplicateProjectionEventHandlerServiceException, eventType.FullName, Name));
        }

        _handlerEventTypes.Add(eventType);
        _eventTypes.Add(eventType);
    }

    public bool HandlesEventType(Type eventType)
    {
        return _eventTypes.Contains(Guard.AgainstNull(eventType));
    }

    public bool TryGetDelegate(Type eventType, [MaybeNullWhen(false)] out ProjectionDelegate handler)
    {
        if (!_delegates.ContainsKey(Guard.AgainstNull(eventType)))
        {
            handler = null;

            return false;
        }

        handler = _delegates[eventType];

        return true;
    }

    public void AddEventHandler(Type eventType, object handler)
    {
        Guard.AgainstNull(eventType);
        Guard.AgainstNull(handler);

        if (_delegates.ContainsKey(eventType))
        {
            throw new InvalidOperationException(string.Format(Resources.ProjectionDelegateEventTypeAlreadyRegisteredException, eventType.FullName, Name));
        }

        if (_handlerEventTypes.Contains(eventType))
        {
            throw new InvalidOperationException(string.Format(Resources.ProjectionHandlerEventTypeAlreadyRegisteredException, eventType.FullName, Name));
        }

        _eventTypes.Add(eventType);
    }
}