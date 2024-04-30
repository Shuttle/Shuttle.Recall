using System;
using System.IO;
using System.Threading.Tasks;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Serialization;

namespace Shuttle.Recall
{
    public interface IDeserializeEventObserver : IPipelineObserver<OnDeserializeEvent>
    {
    }

    public class DeserializeEventObserver : IDeserializeEventObserver
    {
        private readonly ISerializer _serializer;

        public DeserializeEventObserver(ISerializer serializer)
        {
            Guard.AgainstNull(serializer, nameof(serializer));

            _serializer = serializer;
        }

        public void Execute(OnDeserializeEvent pipelineEvent)
        {
            ExecuteAsync(pipelineEvent, true).GetAwaiter().GetResult();
        }

        public async Task ExecuteAsync(OnDeserializeEvent pipelineEvent)
        {
            await ExecuteAsync(pipelineEvent, false).ConfigureAwait(false);
        }

        private async Task ExecuteAsync(OnDeserializeEvent pipelineEvent, bool sync)
        {
            var state = Guard.AgainstNull(pipelineEvent, nameof(pipelineEvent)).Pipeline.State;
            var eventEnvelope = state.GetEventEnvelope();

            Guard.AgainstNull(eventEnvelope, nameof(eventEnvelope));

            using (var stream = new MemoryStream(eventEnvelope.Event))
            {
                state.SetEvent(new DomainEvent(sync ? _serializer.Deserialize(Type.GetType(eventEnvelope.AssemblyQualifiedName, true, true), stream) : await _serializer.DeserializeAsync(Type.GetType(eventEnvelope.AssemblyQualifiedName, true, true), stream), eventEnvelope.Version));
            }
        }
    }
}