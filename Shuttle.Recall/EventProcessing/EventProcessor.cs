using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Shuttle.Core.Container;
using Shuttle.Core.Contract;
using Shuttle.Core.Logging;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Threading;

namespace Shuttle.Recall
{
    public class EventProcessor : IEventProcessor, IThreadState
    {
        private readonly IEventStoreConfiguration _configuration;
        private readonly object _lock = new object();
        private readonly ILog _log;
        private readonly IPipelineFactory _pipelineFactory;

        private readonly Dictionary<Guid, ProjectionAggregation> _projectionAggregations =
            new Dictionary<Guid, ProjectionAggregation>();

        private readonly Dictionary<string, Projection> _projections = new Dictionary<string, Projection>();
        private readonly ConcurrentQueue<Projection> _projectionsQueue = new ConcurrentQueue<Projection>();
        private readonly Guid _projectionsQueueId = Guid.NewGuid();
        private readonly IProjectionRepository _repository;

        private readonly Thread _sequenceNumberTailThread;
        private IProcessorThreadPool _processorThreadPool;
        private volatile bool _started;

        public EventProcessor(IEventStoreConfiguration configuration, IPipelineFactory pipelineFactory,
            IProjectionRepository repository)
        {
            Guard.AgainstNull(configuration, nameof(configuration));
            Guard.AgainstNull(pipelineFactory, nameof(pipelineFactory));
            Guard.AgainstNull(repository, nameof(repository));

            _pipelineFactory = pipelineFactory;
            _repository = repository;
            _configuration = configuration;

            _sequenceNumberTailThread = new Thread(SequenceNumberTailThreadWorker);

            _log = Log.For(this);
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

            _sequenceNumberTailThread.Start();

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

            _sequenceNumberTailThread?.Join();
        }

        public bool Started => _started;

        public void AddProjection(Projection projection)
        {
            Guard.AgainstNull(projection, nameof(Projection));

            if (_started)
            {
                throw new EventProcessingException(Resources.ExceptionEventProcessorStarted);
            }

            if (_projections.ContainsKey(projection.Name))
            {
                throw new EventProcessingException(string.Format(Resources.DuplicateProjectionName,
                    projection.Name));
            }

            if (!ShouldAddProjection(projection))
            {
                return;
            }

            lock (_lock)
            {
                AssignToAggregation(projection);

                _projections.Add(projection.Name, projection);
                _projectionsQueue.Enqueue(projection);
            }
        }

        public Projection GetProjection(string name)
        {
            Guard.AgainstNullOrEmptyString(name, nameof(name));

            var key = name.ToLower();

            if (_projections.ContainsKey(key))
            {
                return _projections[key];
            }

            var projection = _repository.Find(name);

            if (projection == null)
            {
                projection = new Projection(name, 0, Environment.MachineName, AppDomain.CurrentDomain.BaseDirectory);

                _repository.Save(projection);
            }

            AddProjection(projection);

            return projection;
        }

        public Projection GetProjection()
        {
            if (!_projectionsQueue.TryDequeue(out var result))
            {
                return null;
            }

            result.Assigned(_projectionsQueueId);

            return result;
        }

        public ProjectionAggregation GetProjectionAggregation(Guid id)
        {
            var result = _projectionAggregations.ContainsKey(id)
                ? _projectionAggregations[id]
                : null;

            if (result == null)
            {
                throw new InvalidOperationException(string.Format(Resources.MissingProjectionAggregationException, id));
            }

            return result;
        }

        public void ReleaseProjection(Projection projection)
        {
            Guard.AgainstNull(projection, nameof(projection));

            projection.Release(_projectionsQueueId);

            _projectionsQueue.Enqueue(projection);
        }

        public bool Active => _started;

        private void SequenceNumberTailThreadWorker()
        {
            while (_started)
            {
                lock (_lock)
                {
                    foreach (var projectionAggregation in _projectionAggregations)
                    {
                        projectionAggregation.Value.ProcessSequenceNumberTail();
                    }
                }

                ThreadSleep.While(_configuration.SequenceNumberTailThreadWorkerInterval, this);
            }
        }

        private void AssignToAggregation(Projection projection)
        {
            ProjectionAggregation result = null;

            foreach (var projectionAggregation in _projectionAggregations.Values)
            {
                if (!projectionAggregation.IsSatisfiedBy(projection))
                {
                    continue;
                }

                result = projectionAggregation;

                break;
            }

            if (result == null)
            {
                result = new ProjectionAggregation(_configuration.ProjectionEventFetchCount * 3);

                _projectionAggregations.Add(result.Id, result);
            }

            result.Add(projection);
        }

        private bool ShouldAddProjection(Projection projection)
        {
            var result = _configuration.HasActiveProjection(projection.Name) &&
                         (string.IsNullOrEmpty(projection.MachineName) ||
                          Environment.MachineName.Equals(projection.MachineName)) &&
                         (string.IsNullOrEmpty(projection.BaseDirectory) ||
                          AppDomain.CurrentDomain.BaseDirectory.Equals(projection.BaseDirectory));

            _log.Information(result
                ? string.Format(Resources.InformationProjectionActive, projection.Name)
                : string.Format(Resources.InformationProjectionIgnored, projection.Name));

            return result;
        }

        public Projection Get(string name)
        {
            Guard.AgainstNullOrEmptyString(name, nameof(name));

            var projection = _repository.Find(name);

            if (projection == null)
            {
                projection = new Projection(name, 1, Environment.MachineName, AppDomain.CurrentDomain.BaseDirectory);

                _repository.Save(projection);
            }

            return projection;
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