using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Recall;

public class EventStore(IPipelineFactory pipelineFactory, IEventMethodInvoker eventMethodInvoker)
    : IEventStore
{
    private readonly IEventMethodInvoker _eventMethodInvoker = Guard.AgainstNull(eventMethodInvoker);
    private readonly IPipelineFactory _pipelineFactory = Guard.AgainstNull(pipelineFactory);

    public async Task<EventStream> GetAsync(Guid id, Action<EventStreamBuilder>? builder = null, CancellationToken cancellationToken = default)
    {
        if (id.Equals(Guid.Empty))
        {
            return new(Guid.NewGuid(), _eventMethodInvoker);
        }

        var eventStreamBuilder = new EventStreamBuilder();

        builder?.Invoke(eventStreamBuilder);

        var pipeline = await _pipelineFactory.GetPipelineAsync<GetEventStreamPipeline>(cancellationToken);

        try
        {
            return await pipeline.ExecuteAsync(id, eventStreamBuilder).ConfigureAwait(false);
        }
        finally
        {
            await _pipelineFactory.ReleasePipelineAsync(pipeline, cancellationToken);
        }
    }

    public async Task RemoveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var pipeline = await _pipelineFactory.GetPipelineAsync<RemoveEventStreamPipeline>(cancellationToken);

        try
        {
            await pipeline.ExecuteAsync(id).ConfigureAwait(false);
        }
        finally
        {
            await _pipelineFactory.ReleasePipelineAsync(pipeline, cancellationToken);
        }
    }

    public async Task SaveAsync(EventStream eventStream, Action<EventStreamBuilder>? builder = null, CancellationToken cancellationToken = default)
    {
        if (Guard.AgainstNull(eventStream).Removed)
        {
            await RemoveAsync(eventStream.Id, cancellationToken).ConfigureAwait(false);

            return;
        }

        if (!eventStream.ShouldSave())
        {
            return;
        }

        var pipeline = await _pipelineFactory.GetPipelineAsync<SaveEventStreamPipeline>(cancellationToken);

        try
        {
            var eventStreamBuilder = new EventStreamBuilder();

            builder?.Invoke(eventStreamBuilder);

            await pipeline.ExecuteAsync(eventStream, eventStreamBuilder).ConfigureAwait(false);
        }
        finally
        {
            await _pipelineFactory.ReleasePipelineAsync(pipeline, cancellationToken);
        }
    }
}