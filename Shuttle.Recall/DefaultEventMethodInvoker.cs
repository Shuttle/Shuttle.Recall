using System;
using System.Collections.Generic;
using System.Reflection;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Recall
{
    public class DefaultEventMethodInvoker : IEventMethodInvoker
    {
        private readonly BindingFlags _bindingFlags = BindingFlags.Instance;
        private readonly Dictionary<string, MethodInfo> _cache = new Dictionary<string, MethodInfo>();
        private readonly IEventMethodInvokerConfiguration _configuration;
        private readonly object _lock = new object();

        public DefaultEventMethodInvoker(IEventMethodInvokerConfiguration configuration)
        {
            Guard.AgainstNull(configuration, "configuration");

            _configuration = configuration;

            _bindingFlags = _bindingFlags |
                            (_configuration.AllowPublicMethod ? BindingFlags.Public : BindingFlags.NonPublic);
        }

        public void Apply(object instance, IEnumerable<object> events)
        {
            if (events == null)
            {
                return;
            }

            var instanceType = instance.GetType();

            foreach (var @event in events)
            {
                var eventType = @event.GetType();
                var key = string.Concat(instanceType.Name, "/", eventType.Name);

                MethodInfo cachedMethod;

                lock (_lock)
                {
                    if (!_cache.ContainsKey(key))
                    {
                        var method = instanceType.GetMethod(_configuration.EventHandlingMethodName, _bindingFlags, null,
                            new[] {eventType}, null);

                        if (method == null)
                        {
                            throw new UnhandledEventException(string.Format(RecallResources.UnhandledEventException,
                                instanceType.AssemblyQualifiedName, _configuration.EventHandlingMethodName,
                                eventType.AssemblyQualifiedName));
                        }

                        _cache.Add(key, method);
                    }

                    cachedMethod = _cache[key];
                }

                cachedMethod.Invoke(instance, new[] {@event});
            }
        }
    }
}