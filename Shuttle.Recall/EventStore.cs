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

        public EventStream Get(Guid id)
        {
            return GetAsync(id, true).GetAwaiter().GetResult();
        }

        public async Task<EventStream> GetAsync(Guid id)
        {
            return await GetAsync(id, false).ConfigureAwait(false);
        }

        public long Save(EventStream eventStream, Action<EventEnvelopeBuilder> builder = null)
        {
            return SaveAsync(eventStream, builder, true).GetAwaiter().GetResult();
        }

        public async ValueTask<long> SaveAsync(EventStream eventStream, Action<EventEnvelopeBuilder> builder = null)
        {
            return await SaveAsync(eventStream, builder, false).ConfigureAwait(false);
        }

        public void Remove(Guid id)
        {
            RemoveAsync(id, true).GetAwaiter().GetResult();
        }

        public async Task RemoveAsync(Guid id)
        {
            await RemoveAsync(id, false).ConfigureAwait(false);
        }

        private async Task RemoveAsync(Guid id, bool sync)
        {
            var pipeline = _pipelineFactory.GetPipeline<RemoveEventStreamPipeline>();

            try
            {
                if (sync)
                {
                    pipeline.Execute(id);
                }
                else
                {
                    await pipeline.ExecuteAsync(id).ConfigureAwait(false);
                }
            }
            finally
            {
                _pipelineFactory.ReleasePipeline(pipeline);
            }
        }

        private async Task<EventStream> GetAsync(Guid id, bool sync)
        {
            if (id.Equals(Guid.Empty))
            {
                return new EventStream(id, _eventMethodInvoker);
            }

            var pipeline = _pipelineFactory.GetPipeline<GetEventStreamPipeline>();

            try
            {
                return sync
                    ? pipeline.Execute(id)
                    : await pipeline.ExecuteAsync(id).ConfigureAwait(false);
            }
            finally
            {
                _pipelineFactory.ReleasePipeline(pipeline);
            }
        }

        private async ValueTask<long> SaveAsync(EventStream eventStream, Action<EventEnvelopeBuilder> builder, bool sync)
        {
            Guard.AgainstNull(eventStream, nameof(eventStream));

            if (eventStream.Removed)
            {
                if (sync)
                {
                    Remove(eventStream.Id);
                }
                else
                {
                    await RemoveAsync(eventStream.Id).ConfigureAwait(false);
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
                var configurator = new EventEnvelopeBuilder();

                builder?.Invoke(configurator);

                if (sync)
                {
                    pipeline.Execute(eventStream, configurator);
                }
                else
                {
                    await pipeline.ExecuteAsync(eventStream, configurator).ConfigureAwait(false);
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