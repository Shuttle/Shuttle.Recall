using System;
using System.Collections.Generic;
using System.Linq;
using Shuttle.Core.Container;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Threading;

namespace Shuttle.Recall
{
    public class EventProcessor : IEventProcessor
    {
        private class ProjectionAssignment
        {
            public int Count { get; private set; }
            public bool Assigned { get; private set; }

            public void Assign()
            {
                Assigned = true;
            }

            public void Release()
            {
                Count++;
                Assigned = false;
            }
        }

        private readonly object Lock = new object();
        private readonly IEventStoreConfiguration _configuration;
        private readonly IPipelineFactory _pipelineFactory;
        private readonly Dictionary<string, Projection> _projections = new Dictionary<string, Projection>();
        private readonly Dictionary<string, ProjectionAssignment> _projectionsRoundRobin = new Dictionary<string, ProjectionAssignment>();
        private volatile bool _started;
        private IProcessorThreadPool _processorThreadPool;
        private long _tailSequenceNumber = long.MaxValue;

        private readonly KeyValuePair<string, ProjectionAssignment> _defaultRoundRobin =
            default(KeyValuePair<string, ProjectionAssignment>);

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

            if (_projections.ContainsKey(projection.Name))
            {
                throw new EventProcessingException(string.Format(Resources.DuplicateEventQueueName,
                    projection.Name));
            }

            if (projection.SequenceNumber < _tailSequenceNumber)
            {
                _tailSequenceNumber = projection.SequenceNumber;
            }

            lock (Lock)
            {
                _projections.Add(projection.Name, projection);
                _projectionsRoundRobin.Add(projection.Name, new ProjectionAssignment());
            }
        }

        public Projection GetProjection()
        {
            lock (Lock)
            {
                var result = _projectionsRoundRobin
                    .Where(item => !item.Value.Assigned)
                    .OrderBy(item=>item.Value.Count)
                    .FirstOrDefault();

                if (result.Equals(_defaultRoundRobin))
                {
                    return null;
                }

                result.Value.Assign();

                return _projections[result.Key];
            }
        }

        public void ReleaseProjection(string name)
        {
            Guard.AgainstNullOrEmptyString(name, nameof(name));

            if (!_projections.ContainsKey(name))
            {
                return;
            }

            lock (Lock)
            {
                _projectionsRoundRobin[name].Release();
            }
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