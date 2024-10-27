using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;

namespace Shuttle.Recall;

public class ProjectionEventProvider : IProjectionEventProvider
{
    private readonly IEventProcessor _eventProcessor;
    private readonly EventStoreOptions _eventStoreOptions;
    private readonly IPrimitiveEventQuery _query;
    private long _sequenceNumberHead;

    public ProjectionEventProvider(IOptions<EventStoreOptions> eventStoreOptions, IEventProcessor eventProcessor, IPrimitiveEventQuery query)
    {
        _eventStoreOptions = Guard.AgainstNull(Guard.AgainstNull(eventStoreOptions).Value);
        _eventProcessor = Guard.AgainstNull(eventProcessor);
        _query = Guard.AgainstNull(query);
    }

    public async Task<ProjectionEvent> GetAsync(Projection projection)
    {
        Guard.AgainstNull(projection);

        var projectionAggregation = _eventProcessor.GetProjectionAggregation(projection.AggregationId);

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
                var specification = new PrimitiveEvent.Specification()
                    .WithRange(sequenceNumber, _eventStoreOptions.ProjectionEventFetchCount)
                    .AddEventTypes(projectionAggregation.GetEventTypes());

                var primitiveEvents = await _query.SearchAsync(specification).ConfigureAwait(false);

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
                ? new(_sequenceNumberHead)
                : new ProjectionEvent(result);
        }
        finally
        {
            projectionAggregation.Release();
        }
    }
}