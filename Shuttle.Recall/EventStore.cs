using System;
using System.Threading.Tasks;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Recall
{
    public class EventStore : IEventStore
    {
        private readonly IEventMethodInvoker _eventMethodInvoker;
        private readonly IPipelineFactory _pipelineFactory;

        public EventStore(IPipelineFactory pipelineFactory, IEventMethodInvoker eventMethodInvoker)
        {
            Guard.AgainstNull(pipelineFactory, nameof(pipelineFactory));
            Guard.AgainstNull(eventMethodInvoker, nameof(eventMethodInvoker));

            _pipelineFactory = pipelineFactory;
            _eventMethodInvoker = eventMethodInvoker;
        }

        public EventStream Get(Guid id, Action<EventStreamBuilder> builder = null)
        {
            return GetAsync(id, builder, true).GetAwaiter().GetResult();
        }

        public async Task<EventStream> GetAsync(Guid id, Action<EventStreamBuilder> builder = null)
        {
            return await GetAsync(id, builder, false).ConfigureAwait(false);
        }

        public long Save(EventStream eventStream, Action<SaveEventStreamBuilder> builder = null)
        {
            return SaveAsync(eventStream, builder, true).GetAwaiter().GetResult();
        }

        public async ValueTask<long> SaveAsync(EventStream eventStream, Action<SaveEventStreamBuilder> builder = null)
        {
            return await SaveAsync(eventStream, builder, false).ConfigureAwait(false);
        }

        public void Remove(Guid id, Action<EventStreamBuilder> builder = null)
        {
            RemoveAsync(id, builder, true).GetAwaiter().GetResult();
        }

        public async Task RemoveAsync(Guid id, Action<EventStreamBuilder> builder = null)
        {
            await RemoveAsync(id, builder, false).ConfigureAwait(false);
        }

        private async Task RemoveAsync(Guid id, Action<EventStreamBuilder> builder, bool sync)
        {
            var eventStreamBuilder = new EventStreamBuilder();

            builder?.Invoke(eventStreamBuilder);
            
            var pipeline = _pipelineFactory.GetPipeline<RemoveEventStreamPipeline>();

            try
            {
                if (sync)
                {
                    pipeline.Execute(id, eventStreamBuilder);
                }
                else
                {
                    await pipeline.ExecuteAsync(id, eventStreamBuilder).ConfigureAwait(false);
                }
            }
            finally
            {
                _pipelineFactory.ReleasePipeline(pipeline);
            }
        }

        private async Task<EventStream> GetAsync(Guid id, Action<EventStreamBuilder> builder, bool sync)
        {
            if (id.Equals(Guid.Empty))
            {
                return new EventStream(id, _eventMethodInvoker);
            }

            var eventStreamBuilder = new EventStreamBuilder();

            builder?.Invoke(eventStreamBuilder);

            var pipeline = _pipelineFactory.GetPipeline<GetEventStreamPipeline>();

            try
            {
                return sync
                    ? pipeline.Execute(id, eventStreamBuilder)
                    : await pipeline.ExecuteAsync(id, eventStreamBuilder).ConfigureAwait(false);
            }
            finally
            {
                _pipelineFactory.ReleasePipeline(pipeline);
            }
        }

        private async ValueTask<long> SaveAsync(EventStream eventStream, Action<SaveEventStreamBuilder> builder, bool sync)
        {
            Guard.AgainstNull(eventStream, nameof(eventStream));

            if (eventStream.Removed)
            {
                if (sync)
                {
                    Remove(eventStream.Id, (Action<EventStreamBuilder>)builder);
                }
                else
                {
                    await RemoveAsync(eventStream.Id, (Action<EventStreamBuilder>)builder).ConfigureAwait(false);
                }

                return -1;
            }

            if (!eventStream.ShouldSave())
            {
                return -1;
            }

            var pipeline = _pipelineFactory.GetPipeline<SaveEventStreamPipeline>();

            try
            {
                var eventStreamBuilder = new SaveEventStreamBuilder();

                builder?.Invoke(eventStreamBuilder);

                if (sync)
                {
                    pipeline.Execute(eventStream, eventStreamBuilder);
                }
                else
                {
                    await pipeline.ExecuteAsync(eventStream, eventStreamBuilder).ConfigureAwait(false);
                }

                return pipeline.State.GetSequenceNumber();
            }
            finally
            {
                _pipelineFactory.ReleasePipeline(pipeline);
            }
        }
    }
}