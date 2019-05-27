using System;
using System.IO;
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
            var state = pipelineEvent.Pipeline.State;
            var eventEnvelope = state.GetEventEnvelope();

            Guard.AgainstNull(eventEnvelope, nameof(eventEnvelope));

            using (var stream = new MemoryStream(eventEnvelope.Event))
            {
                state.SetEvent(_serializer.Deserialize(Type.GetType(eventEnvelope.AssemblyQualifiedName, true, true),
                    stream));
            }
        }
    }
}