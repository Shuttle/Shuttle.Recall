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

        public EventStream GetEventStream(Guid id)
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

        public EventStream GetEventStreamAll(Guid id)
        {
            throw new NotImplementedException();
        }

        public void RemoveEventStream(Guid id)
        {
            throw new NotImplementedException();
        }

        public void SaveEventStream(EventStream eventStream)
        {
            SaveEventStream(eventStream, null);
        }

        public void SaveEventStream(EventStream eventStream, Action<EventEnvelopeConfigurator> configure)
        {
            throw new NotImplementedException();
        }

        public IEventStore Create(IComponentResolver resolver)
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