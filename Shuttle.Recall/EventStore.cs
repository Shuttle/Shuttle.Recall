using System;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Recall
{
    public class EventStore : IEventStore
    {
        private readonly IPipelineFactory _pipelineFactory;

        public EventStore(IPipelineFactory pipelineFactory)
        {
            Guard.AgainstNull(pipelineFactory, "pipelineFactory");

            _pipelineFactory = pipelineFactory;
        }

        public EventStream Get(Guid id)
        {
            Guard.AgainstNull(id, "id");

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
            Guard.AgainstNull(eventStream, "eventStream");

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

                if (configure != null)
                {
                    configure(configurator);
                }

                pipeline.Execute(eventStream, configurator);
            }
            finally
            {
                _pipelineFactory.ReleasePipeline(pipeline);
            }
        }

        public void Remove(Guid id)
        {
            Guard.AgainstNull(id, "id");

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

		public static IEventStoreConfiguration Register(IComponentRegistry registry)
		{
			Guard.AgainstNull(registry, "registry");

			var configuration = new EventStoreConfiguration();

			new CoreConfigurator().Apply(configuration);

			Register(registry, configuration);

			return configuration;
		}

		public static void Register(IComponentRegistry registry, IEventStoreConfiguration configuration)
		{
			Guard.AgainstNull(registry, "registry");
			Guard.AgainstNull(configuration, "configuration");

			RegisterComponent<IEventStoreConfiguration>(registry, configuration);

			RegisterComponent<ISerializer, DefaultSerializer>(registry);
			RegisterComponent<IProjectionSequenceNumberTracker, ProjectionSequenceNumberTracker>(registry);
			RegisterComponent<IPrimitiveEventQueue, PrimitiveEventQueue>(registry);

			RegisterComponent<TransactionScopeObserver, TransactionScopeObserver>(registry);

			if (!registry.IsRegistered<ITransactionScopeFactory>())
			{
				var transactionScopeConfiguration = configuration.TransactionScope ?? new TransactionScopeConfiguration();

				RegisterComponent<ITransactionScopeFactory>(registry,
					new DefaultTransactionScopeFactory(transactionScopeConfiguration.Enabled,
						transactionScopeConfiguration.IsolationLevel,
						TimeSpan.FromSeconds(transactionScopeConfiguration.TimeoutSeconds)));
			}

			RegisterComponent<IPipelineFactory, DefaultPipelineFactory>(registry);

			var reflectionService = new ReflectionService();

			foreach (var type in reflectionService.GetTypes<IPipeline>(typeof(EventStore).Assembly))
			{
				if (type.IsInterface || registry.IsRegistered(type))
				{
					continue;
				}

				registry.Register(type, type, Lifestyle.Transient);
			}

			foreach (var type in reflectionService.GetTypes<IPipelineObserver>(typeof(EventStore).Assembly))
			{
				if (type.IsInterface || registry.IsRegistered(type))
				{
					continue;
				}

				registry.Register(type, type, Lifestyle.Singleton);
			}

			registry.Register<IEventStore, EventStore>();
			registry.Register<IEventProcessor, EventProcessor>();
		}

		private static void RegisterComponent<TDependency, TImplementation>(IComponentRegistry registry)
			where TDependency : class
			where TImplementation : class, TDependency
		{
			if (registry.IsRegistered(typeof(TDependency)))
			{
				return;
			}

			registry.Register<TDependency, TImplementation>();
		}

		private static void RegisterComponent<TDependency>(IComponentRegistry registry, TDependency instance)
			where TDependency : class
		{
			if (registry.IsRegistered(typeof(TDependency)))
			{
				return;
			}

			registry.Register(instance);
		}

		public static IEventStore Create(IComponentResolver resolver)
        {
            Guard.AgainstNull(resolver, "resolver");

            var configuration = resolver.Resolve<IEventStoreConfiguration>();

            if (configuration == null)
            {
                throw new InvalidOperationException(string.Format(InfrastructureResources.TypeNotRegisteredException,
                    typeof(IEventStoreConfiguration).FullName));
            }

            configuration.Assign(resolver);

            var defaultPipelineFactory = resolver.Resolve<IPipelineFactory>() as DefaultPipelineFactory;

            if (defaultPipelineFactory != null)
            {
                defaultPipelineFactory.Assign(resolver);
            }

            return resolver.Resolve<IEventStore>();
        }
    }
}