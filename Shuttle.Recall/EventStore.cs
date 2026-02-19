using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;

namespace Shuttle.Recall;

public class EventStore(IGetEventStreamPipeline getEventStreamPipeline, IRemoveEventStreamPipeline removeEventStreamPipeline, ISaveEventStreamPipeline saveEventStreamPipeline, IEventMethodInvoker eventMethodInvoker, IOptions<RecallOptions> recallOptions, ILogger<EventStore> logger)
    : IEventStore
{
    private readonly IEventMethodInvoker _eventMethodInvoker = Guard.AgainstNull(eventMethodInvoker);
    private readonly ILogger<EventStore> _logger = Guard.AgainstNull(logger);
    private readonly RecallOptions _recallOptions = Guard.AgainstNull(Guard.AgainstNull(recallOptions).Value);

    public async Task<EventStream> GetAsync(Guid id, Action<EventStreamBuilder>? builder = null, CancellationToken cancellationToken = default)
    {
        if (id.Equals(Guid.Empty))
        {
            return new(Guid.NewGuid(), _eventMethodInvoker);
        }

        var eventStreamBuilder = new EventStreamBuilder();

        builder?.Invoke(eventStreamBuilder);

        LogMessage.EventStoreGet(_logger, id);

        await _recallOptions.Operation.InvokeAsync(new($"[GetAsync] : id = '{id}'"), cancellationToken);

        return await Guard.AgainstNull(getEventStreamPipeline).ExecuteAsync(id, eventStreamBuilder).ConfigureAwait(false);
    }

    public async Task RemoveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        LogMessage.EventStoreRemove(_logger, id);

        await _recallOptions.Operation.InvokeAsync(new($"[RemoveAsync] : id = '{id}'"), cancellationToken);

        await Guard.AgainstNull(removeEventStreamPipeline).ExecuteAsync(id).ConfigureAwait(false);
    }

    public async Task<IEnumerable<EventEnvelope>> SaveAsync(EventStream eventStream, Action<EventStreamBuilder>? builder = null, CancellationToken cancellationToken = default)
    {
        if (Guard.AgainstNull(eventStream).Removed)
        {
            await RemoveAsync(eventStream.Id, cancellationToken).ConfigureAwait(false);

            return [];
        }

        if (!eventStream.ShouldSave())
        {
            return [];
        }

        LogMessage.EventStoreSave(_logger, eventStream.Id);

        await _recallOptions.Operation.InvokeAsync(new($"[SaveAsync] : id = '{eventStream.Id}'"), cancellationToken);

        var eventStreamBuilder = new EventStreamBuilder();

        builder?.Invoke(eventStreamBuilder);

        await Guard.AgainstNull(saveEventStreamPipeline).ExecuteAsync(eventStream, eventStreamBuilder).ConfigureAwait(false);

        return saveEventStreamPipeline.State.GetEventEnvelopes() ?? [];
    }
}