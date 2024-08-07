﻿using System.Threading.Tasks;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Recall
{
    public class SaveEventStreamPipeline : Pipeline
    {
        public SaveEventStreamPipeline(IAssembleEventEnvelopesObserver assembleEventEnvelopesObserver, ISavePrimitiveEventsObserver savePrimitiveEventsObserver, IEventStreamObserver eventStreamObserver) 
        {
            RegisterStage("SaveEventStream")
                .WithEvent<OnAssembleEventEnvelopes>()
                .WithEvent<OnAfterAssembleEventEnvelopes>()
                .WithEvent<OnBeforeSavePrimitiveEvents>()
                .WithEvent<OnSavePrimitiveEvents>()
                .WithEvent<OnAfterSavePrimitiveEvents>()
                .WithEvent<OnCommitEventStream>()
                .WithEvent<OnAfterCommitEventStream>();

            RegisterObserver(Guard.AgainstNull(assembleEventEnvelopesObserver, nameof(assembleEventEnvelopesObserver)));
            RegisterObserver(Guard.AgainstNull(savePrimitiveEventsObserver, nameof(savePrimitiveEventsObserver)));
            RegisterObserver(Guard.AgainstNull(eventStreamObserver, nameof(eventStreamObserver)));
        }

        public void Execute(EventStream eventStream, EventStreamBuilder builder)
        {
            ExecuteAsync(eventStream, builder, true).GetAwaiter().GetResult();
        }

        public async Task ExecuteAsync(EventStream eventStream, EventStreamBuilder builder)
        {
            await ExecuteAsync(eventStream, builder, false).ConfigureAwait(false);
        }

        private async Task ExecuteAsync(EventStream eventStream, EventStreamBuilder builder, bool sync)
        {
            State.SetEventStream(Guard.AgainstNull(eventStream, nameof(eventStream)));
            State.SetEventStreamBuilder(builder);

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