using System;
using Shuttle.Core.Container;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.PipelineTransaction;
using Shuttle.Core.Reflection;
using Shuttle.Core.Serialization;
using Shuttle.Core.Transactions;

namespace Shuttle.Recall
{
    public static class ComponentRegistryExtensions
    {
        public static void RegisterEventStore(this IComponentRegistry registry, IEventStoreConfiguration configuration = null)
        {
            Guard.AgainstNull(registry, nameof(registry));

            var eventStoreConfiguration = configuration ?? new EventStoreConfiguration();

            if (configuration == null)
            {
                new CoreConfigurator().Apply(eventStoreConfiguration);
            }

            Guard.AgainstNull(registry, nameof(registry));
            Guard.AgainstNull(eventStoreConfiguration, nameof(eventStoreConfiguration));

            registry.AttemptRegisterInstance(eventStoreConfiguration);

            registry.AttemptRegister<IEventMethodInvokerConfiguration, EventMethodInvokerConfiguration>();
            registry.AttemptRegister<IEventMethodInvoker, DefaultEventMethodInvoker>();
            registry.AttemptRegister<ISerializer, DefaultSerializer>();
            registry.AttemptRegister<IConcurrencyExceptionSpecification, DefaultConcurrencyExceptionSpecification>();

            registry.AttemptRegister<IProjectionEventProvider, ProjectionEventProvider>();

            registry.AttemptRegister<ITransactionScopeObserver, TransactionScopeObserver>();

            if (!registry.IsRegistered<ITransactionScopeFactory>())
            {
                var transactionScopeConfiguration =
                    eventStoreConfiguration.TransactionScope ?? new TransactionScopeConfiguration();

                registry.AttemptRegisterInstance<ITransactionScopeFactory>(
                    new DefaultTransactionScopeFactory(transactionScopeConfiguration.Enabled,
                        transactionScopeConfiguration.IsolationLevel,
                        TimeSpan.FromSeconds(transactionScopeConfiguration.TimeoutSeconds)));
            }

            registry.AttemptRegister<IPipelineFactory, DefaultPipelineFactory>();

            var reflectionService = new ReflectionService();

            foreach (var type in reflectionService.GetTypesAssignableTo<IPipeline>(typeof(EventStore).Assembly))
            {
                if (type.IsInterface || type.IsAbstract || registry.IsRegistered(type))
                {
                    continue;
                }

                registry.Register(type, type, Lifestyle.Transient);
            }

            foreach (var type in reflectionService.GetTypesAssignableTo<IPipelineObserver>(typeof(EventStore).Assembly))
            {
                if (type.IsInterface || type.IsAbstract)
                {
                    continue;
                }

                var interfaceType = type.InterfaceMatching($"I{type.Name}");

                if (interfaceType != null)
                {
                    if (registry.IsRegistered(type))
                    {
                        continue;
                    }

                    registry.Register(interfaceType, type, Lifestyle.Singleton);
                }
                else
                {
                    throw new ApplicationException(string.Format(Resources.ObserverInterfaceMissingException, type.Name));
                }
            }

            registry.AttemptRegister<IEventStore, EventStore>();
            registry.AttemptRegister<IEventProcessor, EventProcessor>();
        }
    }
}