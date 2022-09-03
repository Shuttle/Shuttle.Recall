using System;
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
            Guard.AgainstNull(id, nameof(id));

            if (id.Equals(Guid.Empty))
            {
                return CreateEventStream();
            }

            var pipeline = _pipelineFactory.GetPipeline<GetEventStreamPipeline>();

            try
            {
                return pipeline.Execute(id);
            }
            finally
            {
                _pipelineFactory.ReleasePipeline(pipeline);
            }
        }

        public long Save(EventStream eventStream)
        {
            return Save(eventStream, null);
        }

        public long Save(EventStream eventStream, Action<EventEnvelopeBuilder> builder)
        {
            Guard.AgainstNull(eventStream, nameof(eventStream));

            if (eventStream.Removed)
            {
                Remove(eventStream.Id);

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

                pipeline.Execute(eventStream, configurator);

                return pipeline.State.GetSequenceNumber();
            }
            finally
            {
                _pipelineFactory.ReleasePipeline(pipeline);
            }
        }

        public void Remove(Guid id)
        {
            Guard.AgainstNull(id, nameof(id));

            var pipeline = _pipelineFactory.GetPipeline<RemoveEventStreamPipeline>();

            try
            {
                pipeline.Execute(id);
            }
            finally
            {
                _pipelineFactory.ReleasePipeline(pipeline);
            }
        }

        public EventStream CreateEventStream(Guid id)
        {
            return new EventStream(id, _eventMethodInvoker);
        }

        public EventStream CreateEventStream()
        {
            return CreateEventStream(Guid.NewGuid());
        }
    }
}