using System;
using System.IO;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Recall
{
    public class DeserializeEventObserver : IPipelineObserver<OnDeserializeEvent>
    {
        private readonly ISerializer _serializer;

        public DeserializeEventObserver(ISerializer serializer)
        {
            Guard.AgainstNull(serializer, "serializer");

            _serializer = serializer;
        }

        public void Execute(OnDeserializeEvent pipelineEvent)
        {
            var state = pipelineEvent.Pipeline.State;

            Guard.AgainstNull(state.GetEventEnvelope(), "eventEnvelope");

            var eventEnvelope = state.GetEventEnvelope();

            using (var stream = new MemoryStream(eventEnvelope.Event))
            {
                state.SetEvent(_serializer.Deserialize(Type.GetType(eventEnvelope.AssemblyQualifiedName, true, true), stream));
            }
        }
    }
}