using System;
using Shuttle.Core.Infrastructure;

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
            Guard.AgainstNull(primitiveEventRepository, "primitiveEventRepository");
            Guard.AgainstNull(serializer, "serializer");
            Guard.AgainstNull(concurrenyExceptionSpecification, "concurrenyExceptionSpecification");

            _primitiveEventRepository = primitiveEventRepository;
            _serializer = serializer;
            _concurrenyExceptionSpecification = concurrenyExceptionSpecification;
        }

        public void Execute(OnSavePrimitiveEvents pipelineEvent)
        {
            var state = pipelineEvent.Pipeline.State;
            var eventStream = state.GetEventStream();
            var eventEnvelopes = state.GetEventEnvelopes();

            Guard.AgainstNull(eventStream, "state.GetEventStream()");
            Guard.AgainstNull(eventEnvelopes, "state.GetEventEnvelopes()");

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
                        string.Format(RecallResources.EventStreamConcurrencyException, eventStream.Id, version), ex);
                }

                throw;
            }
        }
    }
}