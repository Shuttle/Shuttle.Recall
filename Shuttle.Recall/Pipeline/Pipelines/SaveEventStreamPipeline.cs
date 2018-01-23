using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Recall
{
    public class SaveEventStreamPipeline : Pipeline
    {
        public SaveEventStreamPipeline(IAssembleEventEnvelopesObserver assembleEventEnvelopesObserver,
            ISavePrimitiveEventsObserver savePrimitiveEventsObserver, IEventStreamObserver eventStreamObserver)
        {
            Guard.AgainstNull(assembleEventEnvelopesObserver, nameof(assembleEventEnvelopesObserver));
            Guard.AgainstNull(savePrimitiveEventsObserver, nameof(savePrimitiveEventsObserver));
            Guard.AgainstNull(eventStreamObserver, nameof(eventStreamObserver));

            RegisterStage("Process")
                .WithEvent<OnAssembleEventEnvelopes>()
                .WithEvent<OnAfterAssembleEventEnvelopes>()
                .WithEvent<OnSavePrimitiveEvents>()
                .WithEvent<OnAfterSavePrimitiveEvents>()
                .WithEvent<OnCommitEventStream>()
                .WithEvent<OnAfterCommitEventStream>();

            RegisterObserver(assembleEventEnvelopesObserver);
            RegisterObserver(savePrimitiveEventsObserver);
            RegisterObserver(eventStreamObserver);
        }

        public void Execute(EventStream eventStream, EventEnvelopeConfigurator configurator)
        {
            Guard.AgainstNull(eventStream, nameof(eventStream));

            State.SetEventStream(eventStream);
            State.SetEventEnvelopeConfigurator(configurator);

            Execute();
        }
    }
}