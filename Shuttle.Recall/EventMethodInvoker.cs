using System.Reflection;
using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;

namespace Shuttle.Recall;

public class EventMethodInvoker(IOptions<EventStoreOptions> eventStreamOptions) : IEventMethodInvoker
{
    private readonly EventStoreOptions _eventStreamOptions = Guard.AgainstNull(Guard.AgainstNull(eventStreamOptions).Value);
    private readonly Dictionary<string, MethodInfo> _cache = new();
    private readonly Lock _lock = new();

    public void Apply(object instance, IEnumerable<object> events)
    {
        var instanceType = Guard.AgainstNull(instance).GetType();

        foreach (var @event in Guard.AgainstNull(events))
        {
            var eventType = @event.GetType();
            var key = string.Concat(instanceType.FullName, "/", eventType.FullName);

            MethodInfo cachedMethod;

            lock (_lock)
            {
                if (!_cache.ContainsKey(key))
                {
                    var method = instanceType.GetMethod(_eventStreamOptions.EventHandlingMethodName, _eventStreamOptions.BindingFlags, null, [eventType], null);

                    if (method == null)
                    {
                        throw new UnhandledEventException(string.Format(Resources.UnhandledEventException, instanceType.AssemblyQualifiedName, _eventStreamOptions.EventHandlingMethodName, eventType.AssemblyQualifiedName));
                    }

                    _cache.Add(key, method);
                }

                cachedMethod = _cache[key];
            }

            cachedMethod.Invoke(instance, [@event]);
        }
    }
}