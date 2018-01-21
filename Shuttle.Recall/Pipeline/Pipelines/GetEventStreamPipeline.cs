using System;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Recall
{
    public class GetEventStreamPipeline : Pipeline
    {
        public GetEventStreamPipeline(GetStreamEventsObserver getStreamEventsObserver,
            AssembleEventStreamObserver assembleEventStreamObserver)
        {
            Guard.AgainstNull(getStreamEventsObserver, nameof(GetStreamEventsObserver));
            Guard.AgainstNull(assembleEventStreamObserver, nameof(assembleEventStreamObserver));

            RegisterStage("Process")
                .WithEvent<OnGetStreamEvents>()
                .WithEvent<OnAfterGetStreamEvents>()
                .WithEvent<OnAssembleEventStream>()
                .WithEvent<OnAfterAssembleEventStream>();

            RegisterObserver(getStreamEventsObserver);
            RegisterObserver(assembleEventStreamObserver);
        }

        public EventStream Execute(Guid id)
        {
            State.SetId(id);

            Execute();

            return State.GetEventStream();
        }
    }
}