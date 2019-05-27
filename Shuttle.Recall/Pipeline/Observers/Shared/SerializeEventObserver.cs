using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Serialization;
using Shuttle.Core.Streams;

namespace Shuttle.Recall
{
    public interface ISerializeEventObserver : IPipelineObserver<OnSerializeEvent>
    {
    }

    public class SerializeEventObserver : ISerializeEventObserver
    {
        private readonly ISerializer _serializer;

        public SerializeEventObserver(ISerializer serializer)
        {
            Guard.AgainstNull(serializer, nameof(serializer));

            _serializer = serializer;
        }

        public void Execute(OnSerializeEvent pipelineEvent)
        {
            var state = pipelineEvent.Pipeline.State;
            var domainEvent = state.GetDomainEvent();
            var eventEnvelope = state.GetEventEnvelope();
            var bytes = _serializer.Serialize(domainEvent.Event).ToBytes();

            state.SetEventBytes(bytes);

            eventEnvelope.Event = bytes;
        }
    }
}