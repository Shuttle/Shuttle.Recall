using System;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Recall
{
    public class EventStore : IEventStore
    {
        private readonly IPipelineFactory _pipelineFactory;

        public EventStore(IPipelineFactory pipelineFactory)
        {
            Guard.AgainstNull(pipelineFactory, "pipelineFactory");

            _pipelineFactory = pipelineFactory;
        }

        public EventStream Get(Guid id)
        {
            Guard.AgainstNull(id, "id");

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
            Guard.AgainstNull(eventStream, "eventStream");

            if (!eventStream.ShouldSave())
            {
                return;
            }

            var pipeline = _pipelineFactory.GetPipeline<SaveEventStreamPipeline>();

            try
            {
                var configurator = new EventEnvelopeConfigurator();

                if (configure != null)
                {
                    configure(configurator);
                }

                pipeline.Execute(eventStream, configurator);
            }
            finally
            {
                _pipelineFactory.ReleasePipeline(pipeline);
            }
        }

        public void Remove(Guid id)
        {
            Guard.AgainstNull(id, "id");

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

        public static IEventStore Create(IComponentResolver resolver)
        {
            Guard.AgainstNull(resolver, "resolver");

            var configuration = resolver.Resolve<IEventStoreConfiguration>();

            if (configuration == null)
            {
                throw new InvalidOperationException(string.Format(InfrastructureResources.TypeNotRegisteredException,
                    typeof(IEventStoreConfiguration).FullName));
            }

            configuration.Assign(resolver);

            var defaultPipelineFactory = resolver.Resolve<IPipelineFactory>() as DefaultPipelineFactory;

            if (defaultPipelineFactory != null)
            {
                defaultPipelineFactory.Assign(resolver);
            }

            return resolver.Resolve<IEventStore>();
        }
    }
}