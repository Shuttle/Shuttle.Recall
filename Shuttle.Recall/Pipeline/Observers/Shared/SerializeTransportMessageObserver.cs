using Shuttle.Core.Infrastructure;

namespace Shuttle.Recall.Shared
{
    public class SerializeEventEnvelopeObserver : IPipelineObserver<OnSerializeEventEnvelope>
    {
        private readonly ISerializer _serializer;

        public SerializeEventEnvelopeObserver(ISerializer serializer)
        {
            Guard.AgainstNull(serializer, "serializer");

            _serializer = serializer;
        }

        public void Execute(OnSerializeEventEnvelope pipelineEvent)
        {
            var state = pipelineEvent.Pipeline.State;
            var transportMessage = state.GetEventEnvelope();

            Guard.AgainstNull(transportMessage, "transportMessage");

            state.SetEventEnvelopeStream(_serializer.Serialize(transportMessage));
        }
    }
}