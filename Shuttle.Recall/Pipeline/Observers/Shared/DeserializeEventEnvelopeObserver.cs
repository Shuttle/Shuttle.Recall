using System.IO;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Serialization;

namespace Shuttle.Recall
{
    public interface IDeserializeEventEnvelopeObserver : IPipelineObserver<OnDeserializeEventEnvelope>
    {
    }

    public class DeserializeEventEnvelopeObserver : IDeserializeEventEnvelopeObserver
    {
        private readonly ISerializer _serializer;

        public DeserializeEventEnvelopeObserver(ISerializer serializer)
        {
            Guard.AgainstNull(serializer, nameof(serializer));

            _serializer = serializer;
        }

        public void Execute(OnDeserializeEventEnvelope pipelineEvent)
        {
            var state = pipelineEvent.Pipeline.State;
            var primitiveEvent = state.GetPrimitiveEvent();

            Guard.AgainstNull(primitiveEvent, nameof(primitiveEvent));

            EventEnvelope eventEnvelope;

            using (var stream = new MemoryStream(primitiveEvent.EventEnvelope))
            {
                eventEnvelope =
                    (EventEnvelope) _serializer.Deserialize(typeof(EventEnvelope), stream);
            }

            state.SetEventEnvelope(eventEnvelope);
            state.SetEventBytes(eventEnvelope.Event);

            eventEnvelope.AcceptInvariants();
        }
    }
}