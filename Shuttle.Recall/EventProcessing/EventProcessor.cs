using System;
using System.Collections.Generic;
using System.Linq;
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

        private readonly ILog _log;
        private readonly object _lock = new object();
        private readonly IEventStoreConfiguration _configuration;
        private readonly IPipelineFactory _pipelineFactory;
        private IReadOnlyCollection<Type> _eventTypes = new List<Type>();
        private readonly Dictionary<Guid, ProjectionAggregation> _projectionAggregations = new Dictionary<Guid, ProjectionAggregation>();
        private readonly Dictionary<string, Projection> _projections = new Dictionary<string, Projection>();
        private readonly Dictionary<string, ProjectionAssignment> _projectionsRoundRobin = new Dictionary<string, ProjectionAssignment>();
        private volatile bool _started;
        private IProcessorThreadPool _processorThreadPool;

        private readonly Thread _sequenceNumberTailThread;

        private readonly KeyValuePair<string, ProjectionAssignment> _defaultRoundRobin =
            default(KeyValuePair<string, ProjectionAssignment>);

        public EventProcessor(IEventStoreConfiguration configuration, IPipelineFactory pipelineFactory)
        {
            Guard.AgainstNull(configuration, nameof(configuration));
            Guard.AgainstNull(pipelineFactory, nameof(pipelineFactory));

            _pipelineFactory = pipelineFactory;
            _configuration = configuration;

            _sequenceNumberTailThread = new Thread(SequenceNumberTailThreadWorker);

            _log = Log.For(this);
        }

        private void SequenceNumberTailThreadWorker()
        {
            while (_started)
            {
            }
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

            _sequenceNumberTailThread.Start();

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
                throw new EventProcessingException(string.Format(Resources.EventProcessorStartedCannotAddQueue,
                    projection.Name));
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
                _projectionsRoundRobin.Add(projection.Name, new ProjectionAssignment());

                var eventTypes = new List<Type>(_eventTypes);

                eventTypes.AddRange(projection.EventTypes);

                _eventTypes = eventTypes.AsReadOnly();
            }
        }

        private void AssignToAggregation(Projection projection)
        {
            ProjectionAggregation aggregation = null;

            foreach (var projectionAggregation in _projectionAggregations.Values)
            {
                if (!projectionAggregation.IsSatisfiedBy(projection))
                {
                    continue;
                }

                aggregation = projectionAggregation;

                break;
            }

            if (aggregation == null)
            {
                aggregation = new ProjectionAggregation(_configuration.ProjectionAggregationTolerance);

                aggregation.Add(projection);
            }

            _projectionAggregations.Add(aggregation.Id, aggregation);
        }

        private bool ShouldAddProjection(Projection projection)
        {
            var result = _configuration.HasActiveProjection(projection.Name) &&
                         (string.IsNullOrEmpty(projection.MachineName) || Environment.MachineName.Equals(projection.MachineName)) &&
                         (string.IsNullOrEmpty(projection.BaseDirectory) || AppDomain.CurrentDomain.BaseDirectory.Equals(projection.BaseDirectory));

            _log.Information(result
                ? string.Format(Resources.InformationProjectionActive, projection.Name)
                : string.Format(Resources.InformationProjectionIgnored, projection.Name));

            return result;
        }

        public Projection GetProjection()
        {
            lock (_lock)
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

        public void ReleaseProjection(string name)
        {
            Guard.AgainstNullOrEmptyString(name, nameof(name));

            if (!_projections.ContainsKey(name))
            {
                return;
            }

            lock (_lock)
            {
                _projectionsRoundRobin[name].Release();
            }
        }

        public IEnumerable<Type> EventTypes => _eventTypes;

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