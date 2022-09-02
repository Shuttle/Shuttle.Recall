using System;
using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;

namespace Shuttle.Recall
{
    public class ProjectionEventProvider : IProjectionEventProvider
    {
        private readonly IEventProcessor _eventProcessor;
        private readonly IPrimitiveEventQuery _query;
        private readonly EventStoreOptions _eventStoreOptions;
        private long _sequenceNumberHead;

        public ProjectionEventProvider(IOptions<EventStoreOptions> eventStoreOptions, IEventProcessor eventProcessor, IPrimitiveEventQuery query)
        {
            Guard.AgainstNull(eventStoreOptions, nameof(eventStoreOptions));
            Guard.AgainstNull(eventStoreOptions.Value, nameof(eventStoreOptions.Value));
            Guard.AgainstNull(eventProcessor, nameof(eventProcessor));
            Guard.AgainstNull(query, nameof(query));

            _eventStoreOptions = eventStoreOptions.Value;
            _eventProcessor = eventProcessor;
            _query = query;
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
                    foreach (var primitiveEvent in _query.Search(new PrimitiveEvent.Specification().WithSequenceNumberStart(sequenceNumber).WithCount(_eventStoreOptions.ProjectionEventFetchCount).AddEventTypes(projectionAggregation.EventTypes)))
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