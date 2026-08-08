using Microsoft.Extensions.Options;
using Shuttle.Contract;
using Shuttle.Pipelines;
using Shuttle.Serialization;
using Shuttle.Streams;

namespace Shuttle.Recall;

public interface ISavePrimitiveEventsObserver : IPipelineObserver<SavePrimitiveEvents>;

public class SavePrimitiveEventsObserver(IOptions<RecallOptions> recallOptions, IPrimitiveEventRepository primitiveEventRepository, ISerializer serializer, IConcurrencyExceptionSpecification concurrencyExceptionSpecification)
    : ISavePrimitiveEventsObserver
{
    public async Task ExecuteAsync(IPipelineContext<SavePrimitiveEvents> pipelineContext, CancellationToken cancellationToken = default)
    {
        var state = Guard.AgainstNull(pipelineContext).Pipeline.State;
        var eventStream = state.GetEventStream();
        var eventEnvelopes = Guard.AgainstNull(state.GetEventEnvelopes());

        var version = -1;

        try
        {
            var primitiveEvents = new List<PrimitiveEvent>();

            foreach (var eventEnvelope in eventEnvelopes)
            {
                version = eventEnvelope.Version;

                var primitiveEvent = new PrimitiveEvent
                {
                    Id = eventStream.Id,
                    Version = version,
                    CorrelationId = eventStream.CorrelationId,
                    EventEnvelope = await (await serializer.SerializeAsync(eventEnvelope, cancellationToken)).ToBytesAsync(),
                    EventId = eventEnvelope.EventId,
                    EventType = eventEnvelope.EventType,
                    RecordedAt = eventEnvelope.RecordedAt
                };

                primitiveEvents.Add(primitiveEvent);
            }

            await primitiveEventRepository.SaveAsync(primitiveEvents, cancellationToken).ConfigureAwait(false);

            await recallOptions.Value.EventStore.PrimitiveEventsSaved.InvokeAsync(new(primitiveEvents, pipelineContext.Pipeline), cancellationToken);
        }
        catch (Exception ex)
        {
            if (concurrencyExceptionSpecification.IsSatisfiedBy(ex))
            {
                throw new EventStreamConcurrencyException(string.Format(Resources.EventStreamConcurrencyException, eventStream.Id, version), ex);
            }

            throw;
        }
    }
}