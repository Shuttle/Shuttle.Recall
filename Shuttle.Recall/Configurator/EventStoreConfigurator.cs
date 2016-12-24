using System;
using System.Collections.Generic;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Recall
{
    public class EventStoreConfigurator
    {
        private readonly List<Type> _dontRegisterTypes = new List<Type>();
        private readonly IComponentRegistry _registry;

        public EventStoreConfigurator(IComponentRegistry registry)
        {
            Guard.AgainstNull(registry, "registry");

            _registry = registry;
        }

        public EventStoreConfigurator DontRegister<TDependency>()
        {
            if (!_dontRegisterTypes.Contains(typeof (TDependency)))
            {
                _dontRegisterTypes.Add(typeof (TDependency));
            }

            return this;
        }

        public IEventStoreConfiguration Configure()
        {
            var configuration = new EventStoreConfiguration();

            new CoreConfigurator().Apply(configuration);

            var reflectionService = new ReflectionService();

            foreach (var type in reflectionService.GetTypes<IPipelineObserver>())
            {
                if (type.IsInterface || _dontRegisterTypes.Contains(type))
                {
                    continue;
                }

                _registry.Register(type, type, Lifestyle.Singleton);
            }

            return configuration;
        }

        public void RegisterComponents(IEventStoreConfiguration configuration)
        {
            Guard.AgainstNull(configuration, "configuration");

            RegisterDefaultInstance(_registry, configuration);
        }

        private void RegisterDefault<TDependency, TImplementation>(IComponentRegistry registry)
            where TImplementation : class where TDependency : class
        {
            if (_dontRegisterTypes.Contains(typeof (TDependency)))
            {
                return;
            }

            registry.Register<TDependency, TImplementation>();
        }

        private void RegisterDefaultInstance<TDependency>(IComponentRegistry registry, TDependency instance)
            where TDependency : class
        {
            if (_dontRegisterTypes.Contains(typeof (TDependency)))
            {
                return;
            }

            registry.Register(instance);
        }
    }
}