using System;
using System.Threading.Tasks;
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
            ExecuteAsync(id, true).GetAwaiter().GetResult();
        }

        public async Task ExecuteAsync(Guid id)
        {
            await ExecuteAsync(id, false).ConfigureAwait(false);
        }

        private async Task ExecuteAsync(Guid id, bool sync)
        {
            State.SetId(id);

            if (sync)
            {
                Execute();
            }
            else
            {
                await ExecuteAsync();
            }   
        }
    }
}