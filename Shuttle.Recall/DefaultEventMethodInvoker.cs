using System.Collections.Generic;
using System.Reflection;
using Shuttle.Core.Contract;

namespace Shuttle.Recall;

public class DefaultEventMethodInvoker : IEventMethodInvoker
{
    private readonly Dictionary<string, MethodInfo> _cache = new();
    private readonly IEventMethodInvokerConfiguration _configuration;
    private readonly object _lock = new();

    public DefaultEventMethodInvoker(IEventMethodInvokerConfiguration configuration)
    {
        _configuration = Guard.AgainstNull(configuration);
    }

    public void Apply(object instance, IEnumerable<object> events)
    {
        var instanceType = Guard.AgainstNull(instance).GetType();

        foreach (var @event in Guard.AgainstNull(events))
        {
            var eventType = @event.GetType();
            var key = string.Concat(instanceType.Name, "/", eventType.Name);

            MethodInfo cachedMethod;

            lock (_lock)
            {
                if (!_cache.ContainsKey(key))
                {
                    var method = instanceType.GetMethod(_configuration.EventHandlingMethodName,
                        _configuration.BindingFlags, null,
                        new[] { eventType }, null);

                    if (method == null)
                    {
                        throw new UnhandledEventException(string.Format(Resources.UnhandledEventException,
                            instanceType.AssemblyQualifiedName, _configuration.EventHandlingMethodName,
                            eventType.AssemblyQualifiedName));
                    }

                    _cache.Add(key, method);
                }

                cachedMethod = _cache[key];
            }

            cachedMethod.Invoke(instance, new[] { @event });
        }
    }
}