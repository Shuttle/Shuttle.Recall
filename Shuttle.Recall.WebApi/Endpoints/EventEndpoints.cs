using System.Text;
using Asp.Versioning;
using Asp.Versioning.Builder;
using Shuttle.Access.AspNetCore;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Serialization;
using Shuttle.Recall.SqlServer.Storage;
using Shuttle.Recall.WebApi.Models;

namespace Shuttle.Recall.WebApi;

public static class EventEndpoints
{
    private static readonly Type EventEnvelopeType = typeof(EventEnvelope);

    public static WebApplication MapEventEndpoints(this WebApplication app, ApiVersionSet versionSet)
    {
        var apiVersion1 = new ApiVersion(1, 0);

        app.MapPost("/events/search", async (IConfiguration configuration, ISessionContext sessionContext, IPrimitiveEventQuery primitiveEventQuery, ISerializer serializer, Models.PrimitiveEvent.Specification model) =>
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

                var specification = new PrimitiveEvent.Specification()
                    .WithSequenceNumberStart(model.SequenceNumberStart)
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

                    result.Add(new()
                    {
                        PrimitiveEvent = primitiveEvent,
                        EventEnvelope = eventEnvelope,
                        DomainEvent = Encoding.UTF8.GetString(eventEnvelope.Event)
                    });
                }

                return Results.Ok(new EventStoreResponse<Event>
                {
                    Items = result
                });
            })
            .WithTags("Events")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1);

        return app;
    }
}