using System.IO;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Recall
{
    public class DeserializeEventEnvelopeObserver : IPipelineObserver<OnDeserializeEventEnvelope>
    {
        private readonly ISerializer _serializer;

        public DeserializeEventEnvelopeObserver(ISerializer serializer)
        {
            Guard.AgainstNull(serializer, "serializer");

            _serializer = serializer;
        }

        public void Execute(OnDeserializeEventEnvelope pipelineEvent)
        {
            var state = pipelineEvent.Pipeline.State;
            var primitiveEvent = state.GetPrimitiveEvent();

            Guard.AgainstNull(primitiveEvent, "primitiveEvent");

            EventEnvelope eventEnvelope;

            using (var stream = new MemoryStream(primitiveEvent.EventEnvelope))
            {
                eventEnvelope =
                    (EventEnvelope)_serializer.Deserialize(typeof(EventEnvelope), stream);
            }

            state.SetEventEnvelope(eventEnvelope);
            state.SetEventBytes(eventEnvelope.Event);

            eventEnvelope.AcceptInvariants();
        }
    }
}