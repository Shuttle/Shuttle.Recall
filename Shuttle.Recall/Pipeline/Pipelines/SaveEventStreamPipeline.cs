using System;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Recall
{
    public class SaveEventStreamPipeline : Pipeline
    {
        public SaveEventStreamPipeline(AssembleEventEnvelopesObserver assembleEventEnvelopesObserver, SavePrimitiveEventsObserver savePrimitiveEventsObserver)
        {
            Guard.AgainstNull(assembleEventEnvelopesObserver, "assembleEventEnvelopesObserver");
            Guard.AgainstNull(savePrimitiveEventsObserver, "savePrimitiveEventsObserver");

            RegisterStage("Process")
                .WithEvent<OnAssembleEventEnvelopes>()
                .WithEvent<OnAfterAssembleEventEnvelopes>()
                .WithEvent<OnSavePrimitiveEvents>()
                .WithEvent<OnAfterSavePrimitiveEvents>();

            RegisterObserver(assembleEventEnvelopesObserver);
            RegisterObserver(savePrimitiveEventsObserver);
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