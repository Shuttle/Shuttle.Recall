using Shuttle.Core.Infrastructure;

namespace Shuttle.Recall
{
    public class SavePrimitiveEventsObserver : IPipelineObserver<OnSavePrimitiveEvents>
    {
        private readonly IPrimitiveEventRepository _primitiveEventRepository;
        private readonly ISerializer _serializer;

        public SavePrimitiveEventsObserver(IPrimitiveEventRepository primitiveEventRepository, ISerializer serializer)
        {
            Guard.AgainstNull(primitiveEventRepository, "primitiveEventRepository");
            Guard.AgainstNull(serializer, "serializer");

            _primitiveEventRepository = primitiveEventRepository;
            _serializer = serializer;
        }

        public void Execute(OnSavePrimitiveEvents pipelineEvent)
        {
            var state = pipelineEvent.Pipeline.State;
            var eventStream = state.GetEventStream();
            var eventEnvelopes = state.GetEventEnvelopes();

            Guard.AgainstNull(eventStream, "state.GetEventStream()");
            Guard.AgainstNull(eventEnvelopes, "state.GetEventEnvelopes()");

            var version = eventStream.Version;

            foreach (var eventEnvelope in eventEnvelopes)
            {
                version = version + 1;

                _primitiveEventRepository.Save(new PrimitiveEvent
                {
                    Id = state.GetId(),
                    EventEnvelope = _serializer.Serialize(eventEnvelope).ToBytes(),
                    EventType = eventEnvelope.EventType,
                    Version = version,
                    DateRegistered = eventEnvelope.EventDate
                });
            }
        }
    }
}