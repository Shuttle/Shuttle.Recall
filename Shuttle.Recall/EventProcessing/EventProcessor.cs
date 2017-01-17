using System;
using System.Collections.Generic;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Recall
{
    public class EventProcessor : IEventProcessor
    {
        private readonly List<Projection> _projections = new List<Projection>();
        private readonly IPipelineFactory _pipelineFactory;
        private readonly List<ProcessorThread> _processorThreads = new List<ProcessorThread>();
        private readonly IEventStoreConfiguration _configuration;
        private volatile bool _started;

        public EventProcessor(IEventStoreConfiguration configuration, IPipelineFactory pipelineFactory)
        {
            Guard.AgainstNull(configuration, "configuration");
            Guard.AgainstNull(pipelineFactory, "pipelineFactory");

            _pipelineFactory = pipelineFactory;

            _configuration = configuration;
        }

        public void Dispose()
        {
            Stop();
        }

        public IEventProcessor Start()
        {
            if (Started)
            {
                return this;
            }

            foreach (var eventProjection in _projections)
            {
                var processorThread = new ProcessorThread(string.Format("Projection-{0}", eventProjection.Name),
                    new ProjectionProcessor(_configuration, _pipelineFactory, eventProjection));

                processorThread.Start();

                _processorThreads.Add(processorThread);
            }

            _started = true;

            return this;
        }

        public void Stop()
        {
            if (!Started)
            {
                return;
            }

            foreach (var processorThread in _processorThreads)
            {
                processorThread.Stop();
            }

            _started = false;
        }

        public bool Started
        {
            get { return _started; }
        }

        public void AddProjection(Projection projection)
        {
            Guard.AgainstNull(projection, "Projection");

            if (_started)
            {
                throw new EventProcessingException(string.Format(RecallResources.EventProcessorStartedCannotAddQueue,
                    projection.Name));
            }

            if (
                _projections.Find(
                    queue => queue.Name.Equals(projection.Name, StringComparison.InvariantCultureIgnoreCase)) !=
                null)
            {
                throw new EventProcessingException(string.Format(RecallResources.DuplicateEventQueueName,
                    projection.Name));
            }

            _projections.Add(projection);
        }

        public static IEventProcessor Create()
        {
            return Create(null);
        }

        public static IEventProcessor Create(IComponentResolver resolver)
        {
            Guard.AgainstNull(resolver, "resolver");

            var configuration = resolver.Resolve<IEventStoreConfiguration>();

            if (configuration == null)
            {
                throw new InvalidOperationException(string.Format(InfrastructureResources.TypeNotRegisteredException,
                    typeof (IEventStoreConfiguration).FullName));
            }

            configuration.Assign(resolver);

            var defaultPipelineFactory = resolver.Resolve<IPipelineFactory>() as DefaultPipelineFactory;

            if (defaultPipelineFactory != null)
            {
                defaultPipelineFactory.Assign(resolver);
            }

            return resolver.Resolve<IEventProcessor>();
        }
    }
}