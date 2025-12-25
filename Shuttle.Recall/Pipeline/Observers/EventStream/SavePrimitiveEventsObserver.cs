using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Serialization;
using Shuttle.Core.Streams;

namespace Shuttle.Recall;

public interface ISavePrimitiveEventsObserver : IPipelineObserver<SavePrimitiveEvents>;

public class SavePrimitiveEventsObserver(IPrimitiveEventRepository primitiveEventRepository, ISerializer serializer, IConcurrencyExceptionSpecification concurrencyExceptionSpecification)
    : ISavePrimitiveEventsObserver
{
    private readonly IConcurrencyExceptionSpecification _concurrencyExceptionSpecification = Guard.AgainstNull(concurrencyExceptionSpecification);
    private readonly IPrimitiveEventRepository _primitiveEventRepository = Guard.AgainstNull(primitiveEventRepository);
    private readonly ISerializer _serializer = Guard.AgainstNull(serializer);

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
                    EventEnvelope = await (await _serializer.SerializeAsync(eventEnvelope, cancellationToken)).ToBytesAsync(),
                    EventId = eventEnvelope.EventId,
                    EventType = eventEnvelope.EventType,
                    DateRegistered = eventEnvelope.EventDate
                };

                primitiveEvents.Add(primitiveEvent);
            }

            var sequenceNumber = await _primitiveEventRepository.SaveAsync(primitiveEvents, cancellationToken).ConfigureAwait(false);

            switch (sequenceNumber)
            {
                case > 0:
                    state.SetSequenceNumber(sequenceNumber);
                    break;
                case < 1:
                    state.SetSequenceNumber(await _primitiveEventRepository.GetSequenceNumberAsync(eventStream.Id, cancellationToken).ConfigureAwait(false));
                    break;
            }
        }
        catch (Exception ex)
        {
            if (_concurrencyExceptionSpecification.IsSatisfiedBy(ex))
            {
                throw new EventStreamConcurrencyException(string.Format(Resources.EventStreamConcurrencyException, eventStream.Id, version), ex);
            }

            throw;
        }
    }
}