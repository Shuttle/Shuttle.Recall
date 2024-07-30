using System;
using System.Threading.Tasks;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Recall
{
    public class GetEventStreamPipeline : Pipeline
    {
        public GetEventStreamPipeline(IGetStreamEventsObserver getStreamEventsObserver, IAssembleEventStreamObserver assembleEventStreamObserver)
        {
            RegisterStage("GetEventStream")
                .WithEvent<OnBeforeGetStreamEvents>()
                .WithEvent<OnGetStreamEvents>()
                .WithEvent<OnAfterGetStreamEvents>()
                .WithEvent<OnAssembleEventStream>()
                .WithEvent<OnAfterAssembleEventStream>();

            RegisterObserver(Guard.AgainstNull(getStreamEventsObserver, nameof(getStreamEventsObserver)));
            RegisterObserver(Guard.AgainstNull(assembleEventStreamObserver, nameof(assembleEventStreamObserver)));
        }

        public EventStream Execute(Guid id, EventStreamBuilder builder)
        {
            return ExecuteAsync(id, builder, true).GetAwaiter().GetResult();
        }

        public async Task<EventStream> ExecuteAsync(Guid id, EventStreamBuilder builder)
        {
            return await ExecuteAsync(id, builder, false).ConfigureAwait(false);
        }

        private async Task<EventStream> ExecuteAsync(Guid id, EventStreamBuilder builder, bool sync)
        {
            State.SetId(id);
            State.SetEventStreamBuilder(builder);

            if (sync)
            {
                Execute();
            }
            else
            {
                await ExecuteAsync().ConfigureAwait(false);
            }   

            return State.GetEventStream();
        }
    }
}