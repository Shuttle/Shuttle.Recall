using System;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Recall
{
    public class SaveEventStreamPipeline : Pipeline
    {
        public SaveEventStreamPipeline(AssembleEventEnvelopesObserver assembleEventEnvelopesObserver, SavePrimitiveEventsObserver savePrimitiveEventsObserver, EventStreamObserver eventStreamObserver)
        {
            Guard.AgainstNull(assembleEventEnvelopesObserver, "assembleEventEnvelopesObserver");
            Guard.AgainstNull(savePrimitiveEventsObserver, "savePrimitiveEventsObserver");
            Guard.AgainstNull(eventStreamObserver, "eventStreamObserver");

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
            Guard.AgainstNull(eventStream, "eventStream");

            State.SetEventStream(eventStream);
            State.SetEventEnvelopeConfigurator(configurator);

            Execute();
        }
    }
}