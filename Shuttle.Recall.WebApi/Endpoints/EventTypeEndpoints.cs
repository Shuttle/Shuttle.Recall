using Asp.Versioning;
using Asp.Versioning.Builder;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;
using Shuttle.Recall.Sql.Storage;
using Shuttle.Recall.WebApi.Models;
using EventType = Shuttle.Recall.WebApi.Models.EventType;

namespace Shuttle.Recall.WebApi;

public static class EventTypeEndpoints
{
    public static WebApplication MapEventTypeEndpoints(this WebApplication app, ApiVersionSet versionSet)
    {
        var apiVersion1 = new ApiVersion(1, 0);

        app.MapPost("/eventtypes/search", async (HttpContext httpContext, IAccessService accessService, IDatabaseContextFactory databaseContextFactory, IEventTypeQuery eventTypeQuery, EventType.Specification model) =>
            {
                Guard.AgainstNull(httpContext);
                Guard.AgainstNull(accessService);
                Guard.AgainstNull(databaseContextFactory);
                Guard.AgainstNull(eventTypeQuery);

                var sessionTokenResult = httpContext.GetAccessSessionToken();

                if (!sessionTokenResult.Ok || !await accessService.HasPermissionAsync(sessionTokenResult.SessionToken, "recall://default/events"))
                {
                    return Results.Ok(new EventStoreResponse<Sql.Storage.EventType>());
                }

                var specification = new Sql.Storage.EventType.Specification();

                if (model.MaximumRows > 0)
                {
                    specification.WithMaximumRows(model.MaximumRows);
                }

                if (!string.IsNullOrEmpty(model.TypeNameMatch))
                {
                    specification.WithTypeNameMatch(model.TypeNameMatch);
                }

                List<Sql.Storage.EventType> result;

                using (new DatabaseContextScope())
                await using (databaseContextFactory.Create())
                {
                    result = (await eventTypeQuery.SearchAsync(specification)).ToList();
                }

                return Results.Ok(new EventStoreResponse<Sql.Storage.EventType>
                {
                    Authorized = true,
                    Items = result
                });
            })
            .WithTags("EventTypes")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1);

        return app;
    }
}