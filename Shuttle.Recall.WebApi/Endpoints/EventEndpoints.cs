using System.Text;
using Asp.Versioning;
using Asp.Versioning.Builder;
using Shuttle.Access.AspNetCore;
using Shuttle.Access.Query;
using Shuttle.Contract;
using Shuttle.Serialization;
using Shuttle.Recall.SqlServer.Storage;
using Shuttle.Recall.WebApi.Models;

namespace Shuttle.Recall.WebApi;

public static class EventEndpoints
{
    private static readonly Type EventEnvelopeType = typeof(EventEnvelope);

    public static WebApplication MapEventEndpoints(this WebApplication app, ApiVersionSet versionSet)
    {
        var apiVersion1 = new ApiVersion(1, 0);

        app.MapPost("/events/search", PostSearch)
            .WithTags("Events")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1)
            .RequireSession();

        app.MapPost("/events/delete", PostDelete)
            .WithTags("Events")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1)
            .RequireSession();

        return app;
    }

    private static async Task<IResult> PostSearch(IConfiguration configuration, ISessionContext sessionContext, IPrimitiveEventQuery primitiveEventQuery, ISerializer serializer, Models.PrimitiveEvent.Specification model)
    {
        Guard.AgainstNull(configuration);
        Guard.AgainstNull(sessionContext);
        Guard.AgainstNull(primitiveEventQuery);
        Guard.AgainstNull(serializer);

        if (!(sessionContext.Session?.HasPermission("recall://default/events") ?? false))
        {
            return Results.Ok(new EventStoreResponse<Event>());
        }

        var maximumRows = model.MaximumRows;

        if (maximumRows is > 1000 or < 1)
        {
            maximumRows = 1000;
        }

        var specification = new PrimitiveEvent.Specification().WithSequenceNumberStart(model.SequenceNumberStart)
            .WithMaximumRows(maximumRows);

        if (model.Id.HasValue)
        {
            specification.AddId(model.Id.Value);
        }

        foreach (var eventTypeName in model.EventTypes)
        {
            specification.AddEventType(eventTypeName);
        }

        var primitiveEvents = await primitiveEventQuery.SearchAsync(specification);

        var result = new List<Event>();

        foreach (var primitiveEvent in primitiveEvents)
        {
            EventEnvelope eventEnvelope;

            using (var ms = new MemoryStream(primitiveEvent.EventEnvelope))
            {
                eventEnvelope = (EventEnvelope)await serializer.DeserializeAsync(EventEnvelopeType, ms);
            }

            result.Add(new() { PrimitiveEvent = primitiveEvent, EventEnvelope = eventEnvelope, DomainEvent = Encoding.UTF8.GetString(eventEnvelope.Event) });
        }

        return Results.Ok(new EventStoreResponse<Event> { Items = result });
    }
    private static async Task<IResult> PostDelete(ISessionContext sessionContext, IPrimitiveEventRepository primitiveEventRepository, Models.PrimitiveEvent.Specification model)
    {
        Guard.AgainstNull(sessionContext);
        Guard.AgainstNull(primitiveEventRepository);

        if (!(sessionContext.Session?.HasPermission("recall://default/events") ?? false))
        {
            return Results.Ok(new EventStoreResponse<Event>());
        }

        if (model.SequenceNumbers.Count == 0)
        {
            return Results.BadRequest("No sequence numbers have been specified.");
        }

        var specification = new PrimitiveEvent.Specification().AddSequenceNumbers(model.SequenceNumbers);

        await primitiveEventRepository.RemoveAsync(specification);

        return Results.Ok();
    }
}