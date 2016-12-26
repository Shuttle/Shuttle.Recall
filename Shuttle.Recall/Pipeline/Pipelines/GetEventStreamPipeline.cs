using System;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Recall
{
    public class GetEventStreamPipeline : Pipeline
    {
        public GetEventStreamPipeline(GetStreamEventsObserver getStreamEventsObserver, AssembleEventStreamObserver assembleEventStreamObserver)
        {
            Guard.AgainstNull(getStreamEventsObserver, "GetStreamEventsObserver");
            Guard.AgainstNull(assembleEventStreamObserver, "assembleEventStreamObserver");

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