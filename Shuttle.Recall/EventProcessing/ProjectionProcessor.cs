using System;
using System.Collections.Generic;
using Shuttle.Core.Container;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.PipelineTransaction;
using Shuttle.Core.Reflection;
using Shuttle.Core.Serialization;
using Shuttle.Core.Threading;
using Shuttle.Core.Transactions;

namespace Shuttle.Recall
{
    public class ProjectionProcessor : IProcessor
    {
        private readonly IPipelineFactory _pipelineFactory;
        private readonly Projection _projection;
        private readonly IThreadActivity _threadActivity;

        public ProjectionProcessor(IEventStoreConfiguration configuration, IPipelineFactory pipelineFactory,
            Projection projection)
        {
            Guard.AgainstNull(configuration, nameof(configuration));
            Guard.AgainstNull(pipelineFactory, nameof(pipelineFactory));
            Guard.AgainstNull(projection, nameof(Projection));

            _pipelineFactory = pipelineFactory;
            _projection = projection;
            _threadActivity = new ThreadActivity(configuration.DurationToSleepWhenIdle);
        }

        public void Execute(IThreadState state)
        {
            var pipeline = _pipelineFactory.GetPipeline<EventProcessingPipeline>();

            while (state.Active)
            {
                pipeline.State.Clear();
                pipeline.State.SetProjection(_projection);
                pipeline.State.SetThreadState(state);

                pipeline.Execute();

                if (pipeline.State.GetWorking())
                {
                    _threadActivity.Working();
                }
                else
                {
                    _threadActivity.Waiting(state);
                }
            }

            _pipelineFactory.ReleasePipeline(pipeline);
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

            foreach (var type in reflectionService.GetTypesAssignableTo<IPipeline>(typeof(EventProcessor).Assembly))
            {
                if (type.IsInterface || type.IsAbstract || registry.IsRegistered(type))
                {
                    continue;
                }

                registry.Register(type, type, Lifestyle.Transient);
            }

            var observers = new List<Type>();

            foreach (var type in reflectionService.GetTypesAssignableTo<IPipelineObserver>(typeof(EventProcessor).Assembly))
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

                observers.Add(type);
            }

            registry.RegisterCollection(typeof(IPipelineObserver), observers, Lifestyle.Singleton);

            registry.AttemptRegister<IEventStore, EventStore>();
            registry.AttemptRegister<IEventProcessor, EventProcessor>();
        }

    }
}