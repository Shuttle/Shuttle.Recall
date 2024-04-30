using System;
using System.Threading.Tasks;
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
            ExecuteAsync(pipelineEvent, true).GetAwaiter().GetResult();
        }

        public async Task ExecuteAsync(OnSerializeEvent pipelineEvent)
        {
            await ExecuteAsync(pipelineEvent, false).ConfigureAwait(false);
        }

        private async Task ExecuteAsync(OnSerializeEvent pipelineEvent, bool sync)
        {
            var state = Guard.AgainstNull(pipelineEvent, nameof(pipelineEvent)).Pipeline.State;
            var domainEvent = Guard.AgainstNull(state.GetDomainEvent(), StateKeys.DomainEvent);
            var eventEnvelope = Guard.AgainstNull(state.GetEventEnvelope(), StateKeys.EventEnvelope);
            var bytes = sync
                ? _serializer.Serialize(domainEvent.Event).ToBytes()
                : await (await _serializer.SerializeAsync(domainEvent.Event).ConfigureAwait(false)).ToBytesAsync().ConfigureAwait(false);

            state.SetEventBytes(bytes);

            eventEnvelope.Event = bytes;
        }
    }
}