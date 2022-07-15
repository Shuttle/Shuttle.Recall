using System;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Serialization;
using Shuttle.Core.Streams;

namespace Shuttle.Recall
{
    public interface ISavePrimitiveEventsObserver : IPipelineObserver<OnSavePrimitiveEvents>
    {
    }

    public class SavePrimitiveEventsObserver : ISavePrimitiveEventsObserver
    {
        private readonly IConcurrencyExceptionSpecification _concurrencyExceptionSpecification;
        private readonly IPrimitiveEventRepository _primitiveEventRepository;
        private readonly ISerializer _serializer;

        public SavePrimitiveEventsObserver(IPrimitiveEventRepository primitiveEventRepository, ISerializer serializer,
            IConcurrencyExceptionSpecification concurrencyExceptionSpecification)
        {
            Guard.AgainstNull(primitiveEventRepository, nameof(primitiveEventRepository));
            Guard.AgainstNull(serializer, nameof(serializer));
            Guard.AgainstNull(concurrencyExceptionSpecification, nameof(concurrencyExceptionSpecification));

            _primitiveEventRepository = primitiveEventRepository;
            _serializer = serializer;
            _concurrencyExceptionSpecification = concurrencyExceptionSpecification;
        }

        public void Execute(OnSavePrimitiveEvents pipelineEvent)
        {
            var state = pipelineEvent.Pipeline.State;
            var eventStream = state.GetEventStream();
            var eventEnvelopes = state.GetEventEnvelopes();

            Guard.AgainstNull(eventStream, nameof(eventStream));
            Guard.AgainstNull(eventEnvelopes, nameof(eventEnvelopes));

            var version = -1;
            long sequenceNumber = 0;

            try
            {
                foreach (var eventEnvelope in eventEnvelopes)
                {
                    version = eventEnvelope.Version;

                    sequenceNumber = _primitiveEventRepository.Save(new PrimitiveEvent
                    {
                        Id = eventStream.Id,
                        EventEnvelope = _serializer.Serialize(eventEnvelope).ToBytes(),
                        EventId = eventEnvelope.EventId,
                        EventType = eventEnvelope.EventType,
                        IsSnapshot = eventEnvelope.IsSnapshot,
                        Version = version,
                        DateRegistered = eventEnvelope.EventDate
                    });

                    if (sequenceNumber > 0)
                    {
                        state.SetSequenceNumber(sequenceNumber);
                    }
                }

                if (sequenceNumber < 1)
                {
                    state.SetSequenceNumber(_primitiveEventRepository.GetSequenceNumber(eventStream.Id));
                }
            }
            catch (Exception ex)
            {
                if (_concurrencyExceptionSpecification.IsSatisfiedBy(ex))
                {
                    throw new EventStreamConcurrencyException(
                        string.Format(Resources.EventStreamConcurrencyException, eventStream.Id, version), ex);
                }

                throw;
            }
        }
    }
}