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
    public class EventStore : IEventStore
    {
        private readonly IEventMethodInvoker _eventMethodInvoker;
        private readonly IPipelineFactory _pipelineFactory;

        public EventStore(IPipelineFactory pipelineFactory, IEventMethodInvoker eventMethodInvoker)
        {
            Guard.AgainstNull(pipelineFactory, nameof(pipelineFactory));
            Guard.AgainstNull(eventMethodInvoker, nameof(eventMethodInvoker));

            _pipelineFactory = pipelineFactory;
            _eventMethodInvoker = eventMethodInvoker;
        }

        public EventStream Get(Guid id)
        {
            Guard.AgainstNull(id, nameof(id));

            var pipeline = _pipelineFactory.GetPipeline<GetEventStreamPipeline>();

            try
            {
                return pipeline.Execute(id);
            }
            finally
            {
                _pipelineFactory.ReleasePipeline(pipeline);
            }
        }

        public void Save(EventStream eventStream)
        {
            Save(eventStream, null);
        }

        public void Save(EventStream eventStream, Action<EventEnvelopeConfigurator> configure)
        {
            Guard.AgainstNull(eventStream, nameof(eventStream));

            if (eventStream.Removed)
            {
                Remove(eventStream.Id);

                return;
            }

            if (!eventStream.ShouldSave())
            {
                return;
            }

            var pipeline = _pipelineFactory.GetPipeline<SaveEventStreamPipeline>();

            try
            {
                var configurator = new EventEnvelopeConfigurator();

                configure?.Invoke(configurator);

                pipeline.Execute(eventStream, configurator);
            }
            finally
            {
                _pipelineFactory.ReleasePipeline(pipeline);
            }
        }

        public void Remove(Guid id)
        {
            Guard.AgainstNull(id, nameof(id));

            var pipeline = _pipelineFactory.GetPipeline<RemoveEventStreamPipeline>();

            try
            {
                pipeline.Execute(id);
            }
            finally
            {
                _pipelineFactory.ReleasePipeline(pipeline);
            }
        }

        public EventStream CreateEventStream(Guid id)
        {
            return new EventStream(id, _eventMethodInvoker);
        }

        public EventStream CreateEventStream()
        {
            return CreateEventStream(Guid.NewGuid());
        }

        public static IEventStoreConfiguration Register(IComponentRegistry registry)
        {
            Guard.AgainstNull(registry, nameof(registry));

            var configuration = new EventStoreConfiguration();

            new CoreConfigurator().Apply(configuration);

            Register(registry, configuration);

            return configuration;
        }

        public static void Register(IComponentRegistry registry, IEventStoreConfiguration configuration)
        {
            Guard.AgainstNull(registry, nameof(registry));
            Guard.AgainstNull(configuration, nameof(configuration));

            registry.AttemptRegisterInstance(configuration);

            registry.RegistryBoostrap();

            registry.AttemptRegister<IEventMethodInvokerConfiguration, EventMethodInvokerConfiguration>();
            registry.AttemptRegister<IEventMethodInvoker, DefaultEventMethodInvoker>();
            registry.AttemptRegister<ISerializer, DefaultSerializer>();
            registry.AttemptRegister<IProjectionSequenceNumberTracker, ProjectionSequenceNumberTracker>();
            registry.AttemptRegister<IPrimitiveEventQueue, PrimitiveEventQueue>();
            registry.AttemptRegister<IConcurrenyExceptionSpecification, DefaultConcurrenyExceptionSpecification>();

            registry.AttemptRegister<TransactionScopeObserver, TransactionScopeObserver>();

            if (!registry.IsRegistered<ITransactionScopeFactory>())
            {
                var transactionScopeConfiguration =
                    configuration.TransactionScope ?? new TransactionScopeConfiguration();

                registry.AttemptRegisterInstance<ITransactionScopeFactory>(
                    new DefaultTransactionScopeFactory(transactionScopeConfiguration.Enabled,
                        transactionScopeConfiguration.IsolationLevel,
                        TimeSpan.FromSeconds(transactionScopeConfiguration.TimeoutSeconds)));
            }

            registry.AttemptRegister<IPipelineFactory, DefaultPipelineFactory>();

            var reflectionService = new ReflectionService();

            foreach (var type in reflectionService.GetTypesAssignableTo(typeof(EventStore).Assembly))
            {
                if (type.IsInterface || registry.IsRegistered(type))
                {
                    continue;
                }

                registry.Register(type, type, Lifestyle.Transient);
            }

            foreach (var type in reflectionService.GetTypesAssignableTo<IPipelineObserver>(typeof(EventStore).Assembly))
            {
                if (type.IsInterface || registry.IsRegistered(type))
                {
                    continue;
                }

                registry.Register(type, type, Lifestyle.Singleton);
            }

            registry.AttemptRegister<IEventStore, EventStore>();
            registry.AttemptRegister<IEventProcessor, EventProcessor>();
        }

        public static IEventStore Create(IComponentResolver resolver)
        {
            Guard.AgainstNull(resolver, nameof(resolver));

            resolver.ResolverBoostrap();

            var configuration = resolver.Resolve<IEventStoreConfiguration>();

            if (configuration == null)
            {
                throw new InvalidOperationException(string.Format(Core.Container.Resources.TypeNotRegisteredException,
                    typeof(IEventStoreConfiguration).FullName));
            }

            configuration.Assign(resolver);

            if (resolver.Resolve<IPipelineFactory>() is DefaultPipelineFactory defaultPipelineFactory)
            {
                defaultPipelineFactory.Assign(resolver);
            }

            return resolver.Resolve<IEventStore>();
        }
    }
}