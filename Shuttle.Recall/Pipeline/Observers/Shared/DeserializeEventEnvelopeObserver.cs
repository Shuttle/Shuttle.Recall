using System.IO;
using System.Threading;
using System.Threading.Tasks;
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
            ExecuteAsync(pipelineEvent, true).GetAwaiter().GetResult();
        }

        public async Task ExecuteAsync(OnDeserializeEventEnvelope pipelineEvent)
        {
            await ExecuteAsync(pipelineEvent, false).ConfigureAwait(false);
        }

        private async Task ExecuteAsync(OnDeserializeEventEnvelope pipelineEvent, bool sync)
        {
            var state = Guard.AgainstNull(pipelineEvent, nameof(pipelineEvent)).Pipeline.State;
            var primitiveEvent = state.GetPrimitiveEvent();

            Guard.AgainstNull(primitiveEvent, nameof(primitiveEvent));

            EventEnvelope eventEnvelope;

            if (sync)
            {
                using (var stream = new MemoryStream(primitiveEvent.EventEnvelope))
                {
                    eventEnvelope = (EventEnvelope)_serializer.Deserialize(typeof(EventEnvelope), stream);
                }
            }
            else
            {
                using (var stream = new MemoryStream(primitiveEvent.EventEnvelope))
                {
                    eventEnvelope = (EventEnvelope)await _serializer.DeserializeAsync(typeof(EventEnvelope), stream).ConfigureAwait(false);
                }
            }

            state.SetEventEnvelope(eventEnvelope);
            state.SetEventBytes(eventEnvelope.Event);

            eventEnvelope.AcceptInvariants();
        }
    }
}