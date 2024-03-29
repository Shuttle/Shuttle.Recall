using System.Threading.Tasks;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Recall
{
    public class SaveEventStreamPipeline : Pipeline
    {
        public SaveEventStreamPipeline(IAssembleEventEnvelopesObserver assembleEventEnvelopesObserver,
            ISavePrimitiveEventsObserver savePrimitiveEventsObserver, IEventStreamObserver eventStreamObserver)
        {
            Guard.AgainstNull(assembleEventEnvelopesObserver, nameof(assembleEventEnvelopesObserver));
            Guard.AgainstNull(savePrimitiveEventsObserver, nameof(savePrimitiveEventsObserver));
            Guard.AgainstNull(eventStreamObserver, nameof(eventStreamObserver));

            RegisterStage("Process")
                .WithEvent<OnAssembleEventEnvelopes>()
                .WithEvent<OnAfterAssembleEventEnvelopes>()
                .WithEvent<OnBeforeSavePrimitiveEvents>()
                .WithEvent<OnSavePrimitiveEvents>()
                .WithEvent<OnAfterSavePrimitiveEvents>()
                .WithEvent<OnCommitEventStream>()
                .WithEvent<OnAfterCommitEventStream>();

            RegisterObserver(assembleEventEnvelopesObserver);
            RegisterObserver(savePrimitiveEventsObserver);
            RegisterObserver(eventStreamObserver);
        }

        public void Execute(EventStream eventStream, SaveEventStreamBuilder builder)
        {
            ExecuteAsync(eventStream, builder, true).GetAwaiter().GetResult();
        }

        public async Task ExecuteAsync(EventStream eventStream, SaveEventStreamBuilder builder)
        {
            await ExecuteAsync(eventStream, builder, false).ConfigureAwait(false);
        }

        private async Task ExecuteAsync(EventStream eventStream, SaveEventStreamBuilder builder, bool sync)
        {
            State.SetEventStream(Guard.AgainstNull(eventStream, nameof(eventStream)));
            State.SetSaveEventStreamBuilder(builder);

            if (sync)
            {
                Execute();
            }
            else
            {
                await ExecuteAsync().ConfigureAwait(false);
            }
        }
    }
}