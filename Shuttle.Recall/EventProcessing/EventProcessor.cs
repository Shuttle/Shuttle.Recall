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
    public class EventProcessor : IEventProcessor
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
        private CancellationTokenSource _cancellationTokenSource;
        private CancellationToken _cancellationToken;
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

            foreach (var projectionAggregation in _projectionAggregations.Values)
            {
                projectionAggregation.AddEventTypes();
            }

            _cancellationTokenSource = new CancellationTokenSource();
            _cancellationToken = _cancellationTokenSource.Token;

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

            _cancellationTokenSource.Cancel();

            _processorThreadPool?.Dispose();

            _started = false;

            _sequenceNumberTailThread?.Join();
        }

        public bool Started => _started;

        public Projection AddProjection(string name)
        {
            Guard.AgainstNullOrEmptyString(name, nameof(name));

            if (_started)
            {
                throw new EventProcessingException(Resources.ExceptionEventProcessorStarted);
            }

            if (_projections.ContainsKey(name))
            {
                throw new EventProcessingException(string.Format(Resources.DuplicateProjectionName, name));
            }

            if (!ShouldAddProjection(name))
            {
                return null;
            }

            var projection = _repository.Find(name);

            if (projection == null)
            {
                projection = new Projection(name, 0);

                _repository.Save(projection);
            }

            lock (_lock)
            {
                AssignToAggregation(projection);

                _projections.Add(projection.Name.ToLowerInvariant(), projection);
                _projectionsQueue.Enqueue(projection);
            }

            return projection;
        }
        
        public Projection GetProjection(string name)
        {
            Guard.AgainstNullOrEmptyString(name, nameof(name));

            var key = name.ToLowerInvariant();

            if (!_projections.ContainsKey(key))
            {
                throw new EventProcessingException(string.Format(Resources.ProjectionNotRegisteredException, name));
            }

            return _projections[key];
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

                ThreadSleep.While(_configuration.SequenceNumberTailThreadWorkerInterval, _cancellationToken);
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

        private bool ShouldAddProjection(string name)
        {
            var result = _configuration.HasActiveProjection(name);

            _log.Information(result
                ? string.Format(Resources.InformationProjectionActive, name)
                : string.Format(Resources.InformationProjectionIgnored, name));

            return result;
        }

        [Obsolete("Please create an instance using `IComponentResolver.Resolve<IEventProcessor>()`.")]
        public static IEventProcessor Create(IComponentResolver resolver)
        {
            Guard.AgainstNull(resolver, nameof(resolver));

            return resolver.Resolve<IEventProcessor>();

        }
    }
}