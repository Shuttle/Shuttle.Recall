using System;
using System.Collections.Generic;
using System.Linq;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Reflection;

namespace Shuttle.Recall
{
    public class GetEventStreamPipeline : Pipeline
    {
        public GetEventStreamPipeline(IEnumerable<IPipelineObserver> observers)
        {
            Guard.AgainstNull(observers, nameof(observers));

            var list = observers.ToList();

            RegisterStage("Process")
                .WithEvent<OnGetStreamEvents>()
                .WithEvent<OnAfterGetStreamEvents>()
                .WithEvent<OnAssembleEventStream>()
                .WithEvent<OnAfterAssembleEventStream>();

            RegisterObserver(list.Get<IGetStreamEventsObserver>());
            RegisterObserver(list.Get<IAssembleEventStreamObserver>());
        }

        public EventStream Execute(Guid id)
        {
            State.SetId(id);

            Execute();

            return State.GetEventStream();
        }
    }
}