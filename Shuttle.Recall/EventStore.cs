using System;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Recall
{
    public class EventStore : IEventStore
    {
        private readonly IPipelineFactory _pipelineFactory;
        private readonly IEventMethodInvoker _eventMethodInvoker;

        public EventStore(IPipelineFactory pipelineFactory, IEventMethodInvoker eventMethodInvoker)
        {
            Guard.AgainstNull(pipelineFactory, "pipelineFactory");
            Guard.AgainstNull(eventMethodInvoker, "eventMethodInvoker");

            _pipelineFactory = pipelineFactory;
            _eventMethodInvoker = eventMethodInvoker;
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

			registry.AttemptRegister( configuration);

			registry.RegistryBoostrap();

			registry.AttemptRegister<IEventMethodInvokerConfiguration, EventMethodInvokerConfiguration>();
			registry.AttemptRegister<IEventMethodInvoker, DefaultEventMethodInvoker>();
			registry.AttemptRegister<ISerializer, DefaultSerializer>();
			registry.AttemptRegister<IProjectionSequenceNumberTracker, ProjectionSequenceNumberTracker>();
			registry.AttemptRegister<IPrimitiveEventQueue, PrimitiveEventQueue>();

			registry.AttemptRegister<TransactionScopeObserver, TransactionScopeObserver>();

			if (!registry.IsRegistered<ITransactionScopeFactory>())
			{
				var transactionScopeConfiguration = configuration.TransactionScope ?? new TransactionScopeConfiguration();

				registry.AttemptRegister<ITransactionScopeFactory>(
					new DefaultTransactionScopeFactory(transactionScopeConfiguration.Enabled,
						transactionScopeConfiguration.IsolationLevel,
						TimeSpan.FromSeconds(transactionScopeConfiguration.TimeoutSeconds)));
			}

			registry.AttemptRegister<IPipelineFactory, DefaultPipelineFactory>();

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

			registry.AttemptRegister<IEventStore, EventStore>();
			registry.AttemptRegister<IEventProcessor, EventProcessor>();
		}

		public static IEventStore Create(IComponentResolver resolver)
        {
            Guard.AgainstNull(resolver, "resolver");

			resolver.ResolverBoostrap();

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

        public EventStream CreateEventStream(Guid id)
        {
            return new EventStream(id, _eventMethodInvoker);
        }

        public EventStream CreateEventStream()
        {
            return CreateEventStream(Guid.NewGuid());
        }
    }
}