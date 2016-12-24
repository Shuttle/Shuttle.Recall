using Shuttle.Core.Infrastructure;

namespace Shuttle.Recall.Shared
{
    public class SerializeMessageObserver : IPipelineObserver<OnSerializeEvent>
    {
        private readonly ISerializer _serializer;

        public SerializeMessageObserver(ISerializer serializer)
        {
            Guard.AgainstNull(serializer, "serializer");

            _serializer = serializer;
        }

        public void Execute(OnSerializeEvent pipelineEvent)
        {
            var state = pipelineEvent.Pipeline.State;
            var message = state.GetEvent();
            var eventEnvelope = state.GetEventEnvelope();
            var bytes = _serializer.Serialize(message).ToBytes();

            state.SetEventBytes(bytes);

            eventEnvelope.Event = bytes;
        }
    }
}