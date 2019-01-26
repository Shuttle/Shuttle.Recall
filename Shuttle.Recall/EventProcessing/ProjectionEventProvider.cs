using System;
using Shuttle.Core.Contract;

namespace Shuttle.Recall
{
    public class ProjectionEventProvider : IProjectionEventProvider
    {
        private readonly IEventStoreConfiguration _configuration;

        private readonly IEventProcessor _eventProcessor;

        private readonly IPrimitiveEventRepository _repository;

        private long _sequenceNumberHead;

        public ProjectionEventProvider(IEventStoreConfiguration configuration, IEventProcessor eventProcessor,
            IPrimitiveEventRepository repository)
        {
            Guard.AgainstNull(configuration, nameof(configuration));
            Guard.AgainstNull(eventProcessor, nameof(eventProcessor));
            Guard.AgainstNull(repository, nameof(repository));

            _configuration = configuration;
            _eventProcessor = eventProcessor;
            _repository = repository;
        }

        public ProjectionEvent Get(Projection projection)
        {
            Guard.AgainstNull(projection, nameof(projection));

            var projectionAggregation = _eventProcessor.GetProjectionAggregation(projection.AggregationId);

            Guard.AgainstNull(projectionAggregation, nameof(projectionAggregation));

            if (!projectionAggregation.ContainsProjection(projection.Name))
            {
                throw new InvalidOperationException(string.Format(Resources.ProjectionNotInAggregationException, projection.Name, projectionAggregation.Id));
            }

            lock (projectionAggregation)
            {
                var sequenceNumber = projection.SequenceNumber + 1;

                if (!projectionAggregation.ContainsPrimitiveEvent(sequenceNumber))
                {
                    foreach (var primitiveEvent in _repository.Get(sequenceNumber, sequenceNumber + _configuration.ProjectionEventFetchCount,
                        projectionAggregation.EventTypes))
                    {
                        projectionAggregation.AddPrimitiveEvent(primitiveEvent);

                        if (primitiveEvent.SequenceNumber > _sequenceNumberHead)
                        {
                            _sequenceNumberHead = primitiveEvent.SequenceNumber;
                        }
                    }
                }

                var result = projectionAggregation.GetNextPrimitiveEvent(projection.SequenceNumber);

                return result == null
                    ? new ProjectionEvent(_sequenceNumberHead)
                    : new ProjectionEvent(result);
            }
        }
    }
}