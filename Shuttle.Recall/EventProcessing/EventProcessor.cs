using System;
using System.Collections.Generic;
using Shuttle.Core.Container;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Threading;

namespace Shuttle.Recall
{
    public class EventProcessor : IEventProcessor
    {
        private readonly object Lock = new object();
        private readonly IEventStoreConfiguration _configuration;
        private readonly IPipelineFactory _pipelineFactory;
        private readonly List<Projection> _projections = new List<Projection>();
        private int _roundRobinIndex;
        private volatile bool _started;
        private IProcessorThreadPool _processorThreadPool;
        private long _tailSequenceNumber = long.MaxValue;

        public EventProcessor(IEventStoreConfiguration configuration, IPipelineFactory pipelineFactory)
        {
            Guard.AgainstNull(configuration, nameof(configuration));
            Guard.AgainstNull(pipelineFactory, nameof(pipelineFactory));

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

            _processorThreadPool =
                new ProcessorThreadPool(
                    "ControlInboxProcessor",
                    _configuration.ProjectionThreadCount,
                    new ProjectionProcessorFactory(_configuration, _pipelineFactory, this)).Start();

            _started = true;

            return this;
        }

        public void Stop()
        {
            if (!Started)
            {
                return;
            }

            _processorThreadPool?.Dispose();

            _started = false;
        }

        public bool Started => _started;

        public void AddProjection(Projection projection)
        {
            Guard.AgainstNull(projection, nameof(Projection));

            if (_started)
            {
                throw new EventProcessingException(string.Format(Resources.EventProcessorStartedCannotAddQueue,
                    projection.Name));
            }

            if (
                _projections.Find(
                    queue => queue.Name.Equals(projection.Name, StringComparison.InvariantCultureIgnoreCase)) !=
                null)
            {
                throw new EventProcessingException(string.Format(Resources.DuplicateEventQueueName,
                    projection.Name));
            }

            if (projection.SequenceNumber < _tailSequenceNumber)
            {
                _tailSequenceNumber = projection.SequenceNumber;
            }

            _projections.Add(projection);
        }

        public Projection GetProjection()
        {
            var result = _projections[_roundRobinIndex];

            lock (Lock)
            {
                _roundRobinIndex++;

                if (_roundRobinIndex >= _projections.Count)
                {
                    _roundRobinIndex = 0;
                }
            }

            return result;
        }

        public static IEventProcessor Create()
        {
            return Create(null);
        }

        public static IEventProcessor Create(IComponentResolver resolver)
        {
            Guard.AgainstNull(resolver, nameof(resolver));

            var configuration = resolver.Resolve<IEventStoreConfiguration>();

            if (configuration == null)
            {
                throw new InvalidOperationException(string.Format(Core.Container.Resources.TypeNotRegisteredException,
                    typeof(IEventStoreConfiguration).FullName));
            }

            configuration.Assign(resolver);

            var defaultPipelineFactory = resolver.Resolve<IPipelineFactory>() as DefaultPipelineFactory;

            defaultPipelineFactory?.Assign(resolver);

            return resolver.Resolve<IEventProcessor>();
        }
    }
}