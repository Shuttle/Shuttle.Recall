using Shuttle.Core.Infrastructure;

namespace Shuttle.Recall
{
    public class EventEnvelopePipeline : Pipeline
    {
        public EventEnvelopePipeline(AssembleEventEnvelopeObserver assembleEventEnvelopeObserver, CompressEventObserver compressEventObserver, EncryptEventObserver encryptEventObserver, SerializeEventObserver serializeEventObserver)
        {
            Guard.AgainstNull(compressEventObserver, "compressEventObserver");
            Guard.AgainstNull(encryptEventObserver, "encryptEventObserver");
            Guard.AgainstNull(serializeEventObserver, "serializeEventObserver");

            RegisterStage("Get")
                .WithEvent<OnAssembleEventEnvelope>()
                .WithEvent<OnAfterAssembleEventEnvelope>()
                .WithEvent<OnSerializeEvent>()
                .WithEvent<OnAfterSerializeEvent>()
                .WithEvent<OnEncryptEvent>()
                .WithEvent<OnAfterEncryptEvent>()
                .WithEvent<OnCompressEvent>()
                .WithEvent<OnAfterCompressEvent>()
                .WithEvent<OnAssembleEventEnvelope>()
                .WithEvent<OnAfterAssembleEventEnvelope>();

            RegisterObserver(assembleEventEnvelopeObserver);
            RegisterObserver(compressEventObserver);
            RegisterObserver(encryptEventObserver);
            RegisterObserver(serializeEventObserver);
        }

        public EventEnvelope Execute(object @event)
        {
            State.SetEvent(@event);

            Execute();

            return State.GetEventEnvelope();
        }
    }
}