using System;
using System.Threading.Tasks;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Recall;

public class EventStore : IEventStore
{
    private readonly IEventMethodInvoker _eventMethodInvoker;
    private readonly IPipelineFactory _pipelineFactory;

    public EventStore(IPipelineFactory pipelineFactory, IEventMethodInvoker eventMethodInvoker)
    {
        _pipelineFactory = Guard.AgainstNull(pipelineFactory);
        _eventMethodInvoker = Guard.AgainstNull(eventMethodInvoker);
    }

    public async Task<EventStream> GetAsync(Guid id, Action<EventStreamBuilder>? builder = null)
    {
        if (id.Equals(Guid.Empty))
        {
            return new(Guid.NewGuid(), _eventMethodInvoker);
        }

        var eventStreamBuilder = new EventStreamBuilder();

        builder?.Invoke(eventStreamBuilder);

        var pipeline = _pipelineFactory.GetPipeline<GetEventStreamPipeline>();

        try
        {
            return await pipeline.ExecuteAsync(id, eventStreamBuilder).ConfigureAwait(false);
        }
        finally
        {
            _pipelineFactory.ReleasePipeline(pipeline);
        }
    }

    public async Task RemoveAsync(Guid id)
    {
        var pipeline = _pipelineFactory.GetPipeline<RemoveEventStreamPipeline>();

        try
        {
            await pipeline.ExecuteAsync(id).ConfigureAwait(false);
        }
        finally
        {
            _pipelineFactory.ReleasePipeline(pipeline);
        }
    }

    public async ValueTask<long> SaveAsync(EventStream eventStream, Action<EventStreamBuilder>? builder = null)
    {
        if (Guard.AgainstNull(eventStream).Removed)
        {
            await RemoveAsync(eventStream.Id).ConfigureAwait(false);

            return -1;
        }

        if (!eventStream.ShouldSave())
        {
            return -1;
        }

        var pipeline = _pipelineFactory.GetPipeline<SaveEventStreamPipeline>();

        try
        {
            var eventStreamBuilder = new EventStreamBuilder();

            builder?.Invoke(eventStreamBuilder);

            await pipeline.ExecuteAsync(eventStream, eventStreamBuilder).ConfigureAwait(false);

            return pipeline.State.GetSequenceNumber();
        }
        finally
        {
            _pipelineFactory.ReleasePipeline(pipeline);
        }
    }
}