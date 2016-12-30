using Shuttle.Core.Infrastructure;

namespace Shuttle.Recall
{
    public class SerializeEventObserver : IPipelineObserver<OnSerializeEvent>
    {
        private readonly ISerializer _serializer;

        public SerializeEventObserver(ISerializer serializer)
        {
            Guard.AgainstNull(serializer, "serializer");

            _serializer = serializer;
        }

        public void Execute(OnSerializeEvent pipelineEvent)
        {
            var state = pipelineEvent.Pipeline.State;
            var @event = state.GetEvent();
            var eventEnvelope = state.GetEventEnvelope();
            var bytes = _serializer.Serialize(@event).ToBytes();

            state.SetEventBytes(bytes);

            eventEnvelope.Event = bytes;
        }
    }
}