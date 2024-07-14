using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;

namespace Shuttle.Recall
{
    public class ProjectionEventProvider : IProjectionEventProvider
    {
        private readonly IEventProcessor _eventProcessor;
        private readonly EventStoreOptions _eventStoreOptions;
        private readonly IPrimitiveEventQuery _query;
        private long _sequenceNumberHead;

        public ProjectionEventProvider(IOptions<EventStoreOptions> eventStoreOptions, IEventProcessor eventProcessor, IPrimitiveEventQuery query)
        {
            Guard.AgainstNull(eventStoreOptions, nameof(eventStoreOptions));
            _eventStoreOptions = Guard.AgainstNull(eventStoreOptions.Value, nameof(eventStoreOptions.Value));
            _eventProcessor = Guard.AgainstNull(eventProcessor, nameof(eventProcessor));
            _query = Guard.AgainstNull(query, nameof(query));
        }

        public ProjectionEvent Get(Projection projection)
        {
            return GetAsync(projection, true).GetAwaiter().GetResult();
        }

        public async Task<ProjectionEvent> GetAsync(Projection projection)
        {
            return await GetAsync(projection, false).ConfigureAwait(false);
        }

        private async Task<ProjectionEvent> GetAsync(Projection projection, bool sync)
        {
            Guard.AgainstNull(projection, nameof(projection));

            var projectionAggregation = _eventProcessor.GetProjectionAggregation(projection.AggregationId);

            Guard.AgainstNull(projectionAggregation, nameof(projectionAggregation));

            if (!projectionAggregation.ContainsProjection(projection.Name))
            {
                throw new InvalidOperationException(string.Format(Resources.ProjectionNotInAggregationException, projection.Name, projectionAggregation.Id));
            }

            await projectionAggregation.LockAsync();

            try
            {
                var sequenceNumber = projection.SequenceNumber + 1;

                if (!projectionAggregation.ContainsPrimitiveEvent(sequenceNumber))
                {
                    var primitiveEvents = sync
                        ? _query.Search(new PrimitiveEvent.Specification().WithRange(sequenceNumber, _eventStoreOptions.ProjectionEventFetchCount).AddEventTypes(projectionAggregation.GetEventTypes()))
                        : await _query.SearchAsync(new PrimitiveEvent.Specification().WithRange(sequenceNumber, _eventStoreOptions.ProjectionEventFetchCount).AddEventTypes(projectionAggregation.GetEventTypes())).ConfigureAwait(false);

                    foreach (var primitiveEvent in primitiveEvents)
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
            finally
            {
                projectionAggregation.Release();
            }
        }
    }
}