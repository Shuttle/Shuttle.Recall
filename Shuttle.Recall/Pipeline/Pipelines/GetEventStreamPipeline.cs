using System;
using System.Threading.Tasks;
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
            return ExecuteAsync(id, true).GetAwaiter().GetResult();
        }

        public async Task<EventStream> ExecuteAsync(Guid id)
        {
            return await ExecuteAsync(id, false).ConfigureAwait(false);
        }

        private async Task<EventStream> ExecuteAsync(Guid id, bool sync)
        {
            State.SetId(id);

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