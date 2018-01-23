using System;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Recall
{
    public class GetEventStreamPipeline : Pipeline
    {
        public GetEventStreamPipeline(IGetStreamEventsObserver getStreamEventsObserver,
            IAssembleEventStreamObserver assembleEventStreamObserver)
        {
            Guard.AgainstNull(getStreamEventsObserver, nameof(getStreamEventsObserver));
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