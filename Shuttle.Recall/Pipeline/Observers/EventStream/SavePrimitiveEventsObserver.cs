using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Serialization;
using Shuttle.Core.Streams;

namespace Shuttle.Recall;

public interface ISavePrimitiveEventsObserver : IPipelineObserver<OnSavePrimitiveEvents>
{
}

public class SavePrimitiveEventsObserver : ISavePrimitiveEventsObserver
{
    private readonly IConcurrencyExceptionSpecification _concurrencyExceptionSpecification;
    private readonly IPrimitiveEventRepository _primitiveEventRepository;
    private readonly ISerializer _serializer;

    public SavePrimitiveEventsObserver(IPrimitiveEventRepository primitiveEventRepository, ISerializer serializer, IConcurrencyExceptionSpecification concurrencyExceptionSpecification)
    {
        _primitiveEventRepository = Guard.AgainstNull(primitiveEventRepository);
        _serializer = Guard.AgainstNull(serializer);
        _concurrencyExceptionSpecification = Guard.AgainstNull(concurrencyExceptionSpecification);
    }

    public async Task ExecuteAsync(IPipelineContext<OnSavePrimitiveEvents> pipelineContext)
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
                    EventEnvelope = await (await _serializer.SerializeAsync(eventEnvelope)).ToBytesAsync(),
                    EventId = eventEnvelope.EventId,
                    EventType = eventEnvelope.EventType,
                    DateRegistered = eventEnvelope.EventDate
                };

                primitiveEvents.Add(primitiveEvent);
            }

            var sequenceNumber = await _primitiveEventRepository.SaveAsync(primitiveEvents).ConfigureAwait(false);

            switch (sequenceNumber)
            {
                case > 0:
                    state.SetSequenceNumber(sequenceNumber);
                    break;
                case < 1:
                    state.SetSequenceNumber(await _primitiveEventRepository.GetSequenceNumberAsync(eventStream.Id).ConfigureAwait(false));
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