using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Threading;

namespace Shuttle.Recall;

public class EventProcessor : IEventProcessor
{
    private readonly EventStoreOptions _eventStoreOptions;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private readonly IPipelineFactory _pipelineFactory;

    private readonly Dictionary<Guid, ProjectionAggregation> _projectionAggregations = new();

    private readonly Dictionary<string, Projection> _projections = new();
    private readonly ConcurrentQueue<Projection> _projectionsQueue = new();
    private readonly Guid _projectionsQueueId = Guid.NewGuid();

    private readonly Thread _sequenceNumberTailThread;
    private CancellationToken _cancellationToken;
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private IProcessorThreadPool? _eventProcessorThreadPool;

    public EventProcessor(IOptions<EventStoreOptions> eventStoreOptions, IPipelineFactory pipelineFactory)
    {
        _eventStoreOptions = Guard.AgainstNull(Guard.AgainstNull(eventStoreOptions).Value);
        _pipelineFactory = Guard.AgainstNull(pipelineFactory);

        _sequenceNumberTailThread = new(SequenceNumberTailThreadWorker);
    }

    public void Dispose()
    {
        StopAsync().GetAwaiter().GetResult();
    }

    public bool Started { get; private set; }

    public Projection GetProjection(string name)
    {
        var key = Guard.AgainstNullOrEmptyString(name).ToLowerInvariant();

        if (!_projections.TryGetValue(key, out var projection))
        {
            throw new EventProcessingException(string.Format(Resources.ProjectionNotRegisteredException, name));
        }

        return projection;
    }

    public Projection? GetProjection()
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
        var result = _projectionAggregations.TryGetValue(id, out var projectionAggregation)
            ? projectionAggregation
            : null;

        if (result == null)
        {
            throw new InvalidOperationException(string.Format(Resources.MissingProjectionAggregationException, id));
        }

        return result;
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

        _sequenceNumberTailThread.Join();

        await Task.CompletedTask;
    }

    public void ReleaseProjection(Projection projection)
    {
        Guard.AgainstNull(projection);

        projection.Release(_projectionsQueueId);

        _projectionsQueue.Enqueue(projection);
    }

    public async ValueTask DisposeAsync()
    {
        await StopAsync().ConfigureAwait(false);
    }

    public async Task<Projection?> AddProjectionAsync(string name)
    {
        Guard.AgainstNullOrEmptyString(name);

        if (Started)
        {
            throw new EventProcessingException(Resources.AddProjectionEventProcessorStartedException);
        }

        if (_projections.ContainsKey(name))
        {
            throw new EventProcessingException(string.Format(Resources.DuplicateProjectionNameException, name));
        }

        if (!_eventStoreOptions.HasActiveProjection(name))
        {
            return null;
        }

        var pipeline = _pipelineFactory.GetPipeline<AddProjectionPipeline>();

        try
        {
            await pipeline.ExecuteAsync(name).ConfigureAwait(false);
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
        ProjectionAggregation? result = null;

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
            result = new(_eventStoreOptions.ProjectionEventFetchCount * 3, _cancellationToken);

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

    public async Task<IEventProcessor> StartAsync()
    {
        if (Started)
        {
            return this;
        }

        var startupPipeline = _pipelineFactory.GetPipeline<EventProcessorStartupPipeline>();

        await startupPipeline.ExecuteAsync(_cancellationToken).ConfigureAwait(false);

        _cancellationToken = _cancellationTokenSource.Token;

        _eventProcessorThreadPool = startupPipeline.State.Get<IProcessorThreadPool>("EventProcessorThreadPool");

        _sequenceNumberTailThread.Start();

        Started = true;

        return this;
    }
}