using System;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Recall
{
    public class RemoveEventStreamPipeline : Pipeline
    {
        public RemoveEventStreamPipeline(IRemoveEventStreamObserver removeEventStreamObserver)
        {
            Guard.AgainstNull(removeEventStreamObserver, nameof(removeEventStreamObserver));

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