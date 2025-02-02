using System.Text;
using Asp.Versioning;
using Asp.Versioning.Builder;
using Shuttle.Access;
using Shuttle.Access.AspNetCore;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;
using Shuttle.Core.Serialization;
using Shuttle.Recall.Sql.Storage;
using Shuttle.Recall.WebApi.Models;

namespace Shuttle.Recall.WebApi;

public static class EventEndpoints
{
    private static readonly Type EventEnvelopeType = typeof(EventEnvelope);

    public static WebApplication MapEventEndpoints(this WebApplication app, ApiVersionSet versionSet)
    {
        var apiVersion1 = new ApiVersion(1, 0);

        app.MapPost("/events/search", async (HttpContext httpContext, IAccessService accessService, IDatabaseContextFactory databaseContextFactory, IPrimitiveEventQuery primitiveEventQuery, ISerializer serializer, Models.PrimitiveEvent.Specification model) =>
        {
            Guard.AgainstNull(httpContext);
            Guard.AgainstNull(accessService);
            Guard.AgainstNull(databaseContextFactory);
            Guard.AgainstNull(primitiveEventQuery);
            Guard.AgainstNull(serializer);

            var sessionTokenResult = httpContext.GetAccessSessionToken();

            if (!sessionTokenResult.Ok || !await accessService.HasPermissionAsync(sessionTokenResult.SessionToken, "recall://default/events"))
            {
                return Results.Ok(new EventStoreResponse<Event>());
            }

            var maximumRows = model.MaximumRows;

            if (maximumRows is > 1000 or < 1)
            {
                maximumRows = 1000;
            }

            var specification = new PrimitiveEvent.Specification().WithSequenceNumberStart(model.SequenceNumberStart).WithMaximumRows(maximumRows);

            foreach (var eventTypeName in model.EventTypes)
            {
                specification.AddEventType(eventTypeName);
            }

            IEnumerable<PrimitiveEvent> primitiveEvents;

            using (new DatabaseContextScope())
            await using (databaseContextFactory.Create())
            {
                primitiveEvents = await primitiveEventQuery.SearchAsync(specification);
            }

            var result = new List<Event>();

            foreach (var primitiveEvent in primitiveEvents)
            {
                EventEnvelope eventEnvelope;

                using (var ms = new MemoryStream(primitiveEvent.EventEnvelope))
                {
                    eventEnvelope = (EventEnvelope)(await serializer.DeserializeAsync(EventEnvelopeType, ms));
                }

                result.Add(new()
                {
                    PrimitiveEvent = primitiveEvent,
                    EventEnvelope = eventEnvelope,
                    DomainEvent = Encoding.UTF8.GetString(eventEnvelope.Event)
                });
            }

            return Results.Ok(new EventStoreResponse<Event>
            {
                Authorized = true,
                Items = result
            });
        })
        .WithTags("Events")
        .WithApiVersionSet(versionSet)
        .MapToApiVersion(apiVersion1);

        return app;
    }
}