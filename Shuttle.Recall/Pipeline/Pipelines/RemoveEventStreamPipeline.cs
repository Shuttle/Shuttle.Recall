using System;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Recall
{
    public class RemoveEventStreamPipeline : Pipeline
    {
        public RemoveEventStreamPipeline(RemoveEventStreamObserver removeEventStreamObserver)
        {
            Guard.AgainstNull(removeEventStreamObserver, "removeEventStreamObserver");

            RegisterStage("Process")
                .WithEvent<OnRemoveEventStream>()
                .WithEvent<OnAfterRemoveEventStream>();

            RegisterObserver(removeEventStreamObserver);
        }

        public void Execute(Guid id)
        {
            State.SetId(id);

            Execute();
        }
    }
}