using System;
using Shuttle.Core.Container;
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

        public void Save(EventStream eventStream)
        {
            Save(eventStream, null);
        }

        public void Save(EventStream eventStream, Action<EventEnvelopeConfigurator> configure)
        {
            Guard.AgainstNull(eventStream, nameof(eventStream));

            if (eventStream.Removed)
            {
                Remove(eventStream.Id);

                return;
            }

            if (!eventStream.ShouldSave())
            {
                return;
            }

            var pipeline = _pipelineFactory.GetPipeline<SaveEventStreamPipeline>();

            try
            {
                var configurator = new EventEnvelopeConfigurator();

                configure?.Invoke(configurator);

                pipeline.Execute(eventStream, configurator);
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

        [Obsolete("This method has been replaced by `ComponentRegistryExtensions.RegisterEventStore()`.", false)]
        public static IEventStoreConfiguration Register(IComponentRegistry registry)
        {
            return registry.RegisterEventStore();
        }

        [Obsolete("Please create an instance using `IComponentResolver.Resolve<IEventStore>()`.")]
        public static IEventStore Create(IComponentResolver resolver)
        {
            Guard.AgainstNull(resolver, nameof(resolver));

            return resolver.Resolve<IEventStore>();
        }
    }
}