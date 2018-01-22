using System;
using System.Collections.Generic;
using System.Linq;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Reflection;

namespace Shuttle.Recall
{
    public class RemoveEventStreamPipeline : Pipeline
    {
        public RemoveEventStreamPipeline(IEnumerable<IPipelineObserver> observers)
        {
            Guard.AgainstNull(observers, nameof(observers));

            var list = observers.ToList();

            RegisterStage("Process")
                .WithEvent<OnRemoveEventStream>()
                .WithEvent<OnAfterRemoveEventStream>();

            RegisterObserver(list.Get<IRemoveEventStreamObserver>());
        }

        public void Execute(Guid id)
        {
            State.SetId(id);

            Execute();
        }
    }
}