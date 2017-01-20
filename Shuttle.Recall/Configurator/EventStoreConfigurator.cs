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

            RegisterComponents(configuration);

            return configuration;
        }

        public void RegisterComponents(IEventStoreConfiguration configuration)
        {
            Guard.AgainstNull(configuration, "configuration");

            RegisterDefaultInstance(_registry, configuration);

            RegisterDefault<ISerializer, DefaultSerializer>(_registry);
            RegisterDefault<IProjectionSequenceNumberTracker, ProjectionSequenceNumberTracker>(_registry);
            RegisterDefault<IPrimitiveEventQueue, PrimitiveEventQueue>(_registry);

            var transactionScopeConfiguration = configuration.TransactionScope ?? new TransactionScopeConfiguration();

            RegisterDefaultInstance<ITransactionScopeFactory>(_registry,
                new DefaultTransactionScopeFactory(transactionScopeConfiguration.Enabled,
                    transactionScopeConfiguration.IsolationLevel,
                    TimeSpan.FromSeconds(transactionScopeConfiguration.TimeoutSeconds)));

            RegisterDefault<IPipelineFactory, DefaultPipelineFactory>(_registry);

            _registry.Register(typeof(GetEventEnvelopePipeline), typeof(GetEventEnvelopePipeline), Lifestyle.Transient);
            _registry.Register(typeof(AssembleEventEnvelopePipeline), typeof(AssembleEventEnvelopePipeline), Lifestyle.Transient);
            _registry.Register(typeof(GetEventStreamPipeline), typeof(GetEventStreamPipeline), Lifestyle.Transient);
            _registry.Register(typeof(EventProcessingPipeline), typeof(EventProcessingPipeline), Lifestyle.Transient);
            _registry.Register(typeof(SaveEventStreamPipeline), typeof(SaveEventStreamPipeline), Lifestyle.Transient);
            _registry.Register(typeof(RemoveEventStreamPipeline), typeof(RemoveEventStreamPipeline), Lifestyle.Transient);

            var reflectionService = new ReflectionService();

            foreach (var type in reflectionService.GetTypes<IPipelineObserver>())
            {
                if (type.IsInterface || _dontRegisterTypes.Contains(type))
                {
                    continue;
                }

                _registry.Register(type, type, Lifestyle.Singleton);
            }

            _registry.Register<IEventStore, EventStore>();
            _registry.Register<IEventProcessor, EventProcessor>();
        }

        private void RegisterDefault<TDependency, TImplementation>(IComponentRegistry registry)
            where TDependency : class 
            where TImplementation : class, TDependency 
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

        public static IEventStoreConfiguration Configure(IComponentRegistry registry)
        {
            return new EventStoreConfigurator(registry).Configure();
        }
    }
}