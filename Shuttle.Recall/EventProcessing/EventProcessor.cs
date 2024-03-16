using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Threading;

namespace Shuttle.Recall
{
    public class EventProcessor : IEventProcessor
    {
        private readonly EventStoreOptions _eventStoreOptions;
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
        private readonly IPipelineFactory _pipelineFactory;

        private readonly Dictionary<Guid, ProjectionAggregation> _projectionAggregations =
            new Dictionary<Guid, ProjectionAggregation>();

        private readonly Dictionary<string, Projection> _projections = new Dictionary<string, Projection>();
        private readonly ConcurrentQueue<Projection> _projectionsQueue = new ConcurrentQueue<Projection>();
        private readonly Guid _projectionsQueueId = Guid.NewGuid();

        private readonly Thread _sequenceNumberTailThread;
        private CancellationToken _cancellationToken;
        private CancellationTokenSource _cancellationTokenSource;
        private IProcessorThreadPool _eventProcessorThreadPool;

        public EventProcessor(IOptions<EventStoreOptions> eventStoreOptions, IPipelineFactory pipelineFactory)
        {
            Guard.AgainstNull(eventStoreOptions, nameof(eventStoreOptions));

            _eventStoreOptions = Guard.AgainstNull(eventStoreOptions.Value, nameof(eventStoreOptions.Value));
            _pipelineFactory = Guard.AgainstNull(pipelineFactory, nameof(pipelineFactory));

            _sequenceNumberTailThread = new Thread(SequenceNumberTailThreadWorker);
        }

        public bool Asynchronous { get; private set; }

        public void Dispose()
        {
            this.Stop();
        }

        public IEventProcessor Start()
        {
            if (_eventStoreOptions.Asynchronous)
            {
                throw new ApplicationException(Resources.EventProcessorStartAsynchronousException);
            }

            StartAsync(true).GetAwaiter().GetResult();

            return this;
        }

        public bool Started { get; private set; }

        public Projection AddProjection(string name)
        {
            if (_eventStoreOptions.Asynchronous)
            {
                throw new EventProcessingException(Resources.EventProcessorAddProjectionAsynchronousException);
            }

            return AddProjectionAsync(name, true).GetAwaiter().GetResult();
        }

        public async Task<Projection> AddProjectionAsync(string name)
        {
            if (!_eventStoreOptions.Asynchronous)
            {
                throw new EventProcessingException(Resources.EventProcessorAddProjectionSynchronousException);
            }

            return await AddProjectionAsync(name, false).ConfigureAwait(false);
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

        public async Task<IEventProcessor> StartAsync()
        {
            if (!_eventStoreOptions.Asynchronous)
            {
                throw new ApplicationException(Resources.EventProcessorStartSynchronousException);
            }

            await StartAsync(false).ConfigureAwait(false);

            return this;
        }

        public async Task StopAsync()
        {
            if (!Started)
            {
                return;
            }

            _cancellationTokenSource.Cancel();

            _eventProcessorThreadPool?.Dispose();

            Started = false;

            _sequenceNumberTailThread?.Join();

            await Task.CompletedTask;
        }

        public void ReleaseProjection(Projection projection)
        {
            Guard.AgainstNull(projection, nameof(projection));

            projection.Release(_projectionsQueueId);

            _projectionsQueue.Enqueue(projection);
        }

        public async ValueTask DisposeAsync()
        {
            await StopAsync().ConfigureAwait(false);
        }

        private async Task<Projection> AddProjectionAsync(string name, bool sync)
        {
            Guard.AgainstNullOrEmptyString(name, nameof(name));

            if (Started)
            {
                throw new EventProcessingException(Resources.ExceptionEventProcessorStarted);
            }

            if (_projections.ContainsKey(name))
            {
                throw new EventProcessingException(string.Format(Resources.DuplicateProjectionName, name));
            }

            if (!_eventStoreOptions.HasActiveProjection(name))
            {
                return null;
            }

            var pipeline = _pipelineFactory.GetPipeline<AddProjectionPipeline>();

            try
            {
                if (sync)
                {
                    pipeline.Execute(name);
                }
                else
                {
                    await pipeline.ExecuteAsync(name).ConfigureAwait(false);
                }
            }
            finally
            {
                _pipelineFactory.ReleasePipeline(pipeline);
            }

            var projection = pipeline.State.GetProjection();

            await _lock.WaitAsync(_cancellationToken).ConfigureAwait(false);

            try
            {
                AssignToAggregation(projection);

                _projections.Add(projection.Name.ToLowerInvariant(), projection);
                _projectionsQueue.Enqueue(projection);
            }
            finally
            {
                _lock.Release();
            }

            return projection;
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
                result = new ProjectionAggregation(_eventStoreOptions.ProjectionEventFetchCount * 3, _cancellationToken);

                _projectionAggregations.Add(result.Id, result);
            }

            result.Add(projection);
        }

        private void SequenceNumberTailThreadWorker()
        {
            while (Started)
            {
                try
                {
                    _lock.Wait(CancellationToken.None);

                    foreach (var projectionAggregation in _projectionAggregations)
                    {
                        projectionAggregation.Value.ProcessSequenceNumberTail();
                    }

                    Task.Delay(_eventStoreOptions.SequenceNumberTailThreadWorkerInterval, _cancellationToken)
                        .Wait(_cancellationToken);
                }
                catch (OperationCanceledException)
                {
                }
                finally
                {
                    _lock.Release();
                }
            }
        }

        private async Task<IEventProcessor> StartAsync(bool sync)
        {
            if (Started)
            {
                return this;
            }

            Started = true;
            Asynchronous = !sync;

            foreach (var projectionAggregation in _projectionAggregations.Values)
            {
                projectionAggregation.AddEventTypes();
            }

            try
            {
                var startupPipeline = _pipelineFactory.GetPipeline<EventProcessorStartupPipeline>();

                if (sync)
                {
                    startupPipeline.Execute();
                }
                else
                {
                    await startupPipeline.ExecuteAsync().ConfigureAwait(false);
                }

                _cancellationTokenSource = new CancellationTokenSource();
                _cancellationToken = _cancellationTokenSource.Token;

                _eventProcessorThreadPool = startupPipeline.State.Get<IProcessorThreadPool>("EventProcessorThreadPool");

                _sequenceNumberTailThread.Start();
            }
            catch
            {
                Started = false;
                throw;
            }

            return this;
        }
    }
}