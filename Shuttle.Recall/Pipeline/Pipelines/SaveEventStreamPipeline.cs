using System.Collections.Generic;
using System.Linq;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Reflection;

namespace Shuttle.Recall
{
    public class SaveEventStreamPipeline : Pipeline
    {
        public SaveEventStreamPipeline(IEnumerable<IPipelineObserver> observers)
        {
            Guard.AgainstNull(observers, nameof(observers));

            var list = observers.ToList();

            RegisterStage("Process")
                .WithEvent<OnAssembleEventEnvelopes>()
                .WithEvent<OnAfterAssembleEventEnvelopes>()
                .WithEvent<OnSavePrimitiveEvents>()
                .WithEvent<OnAfterSavePrimitiveEvents>()
                .WithEvent<OnCommitEventStream>()
                .WithEvent<OnAfterCommitEventStream>();

            RegisterObserver(list.Get<IAssembleEventEnvelopesObserver>());
            RegisterObserver(list.Get<ISavePrimitiveEventsObserver>());
            RegisterObserver(list.Get<IEventStreamObserver>());
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