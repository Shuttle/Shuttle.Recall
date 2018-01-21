using System;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Serialization;
using Shuttle.Core.Streams;

namespace Shuttle.Recall
{
    public class SavePrimitiveEventsObserver : IPipelineObserver<OnSavePrimitiveEvents>
    {
        private readonly IConcurrenyExceptionSpecification _concurrenyExceptionSpecification;
        private readonly IPrimitiveEventRepository _primitiveEventRepository;
        private readonly ISerializer _serializer;

        public SavePrimitiveEventsObserver(IPrimitiveEventRepository primitiveEventRepository, ISerializer serializer,
            IConcurrenyExceptionSpecification concurrenyExceptionSpecification)
        {
            Guard.AgainstNull(primitiveEventRepository, nameof(primitiveEventRepository));
            Guard.AgainstNull(serializer, nameof(serializer));
            Guard.AgainstNull(concurrenyExceptionSpecification, nameof(concurrenyExceptionSpecification));

            _primitiveEventRepository = primitiveEventRepository;
            _serializer = serializer;
            _concurrenyExceptionSpecification = concurrenyExceptionSpecification;
        }

        public void Execute(OnSavePrimitiveEvents pipelineEvent)
        {
            var state = pipelineEvent.Pipeline.State;
            var eventStream = state.GetEventStream();
            var eventEnvelopes = state.GetEventEnvelopes();

            Guard.AgainstNull(eventStream, nameof(eventStream));
            Guard.AgainstNull(eventEnvelopes, nameof(eventEnvelopes));

            var version = -1;

            try
            {
                foreach (var eventEnvelope in eventEnvelopes)
                {
                    version = eventEnvelope.Version;

                    _primitiveEventRepository.Save(new PrimitiveEvent
                    {
                        Id = eventStream.Id,
                        EventEnvelope = _serializer.Serialize(eventEnvelope).ToBytes(),
                        EventId = eventEnvelope.EventId,
                        EventType = eventEnvelope.EventType,
                        IsSnapshot = eventEnvelope.IsSnapshot,
                        Version = version,
                        DateRegistered = eventEnvelope.EventDate
                    });
                }
            }
            catch (Exception ex)
            {
                if (_concurrenyExceptionSpecification.IsSatisfiedBy(ex))
                {
                    throw new EventStreamConcurrencyException(
                        string.Format(Resources.EventStreamConcurrencyException, eventStream.Id, version), ex);
                }

                throw;
            }
        }
    }
}