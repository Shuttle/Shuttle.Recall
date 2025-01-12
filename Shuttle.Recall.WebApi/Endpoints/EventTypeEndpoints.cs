using System.Text;
using Asp.Versioning;
using Asp.Versioning.Builder;
using Shuttle.Access;
using Shuttle.Access.AspNetCore;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;
using Shuttle.Recall.Sql.Storage;

namespace Shuttle.Recall.WebApi;

public static class EventTypeEndpoints
{
    public static WebApplication MapEventTypeEndpoints(this WebApplication app, ApiVersionSet versionSet)
    {
        var apiVersion1 = new ApiVersion(1, 0);

        app.MapPost("/eventtypes/search", async (HttpContext httpContext, IAccessService accessService, IDatabaseContextFactory databaseContextFactory, IEventTypeQuery eventTypeQuery, Models.EventType.Specification model) =>
        {
            Guard.AgainstNull(httpContext);
            Guard.AgainstNull(accessService);
            Guard.AgainstNull(databaseContextFactory);
            Guard.AgainstNull(eventTypeQuery);

            var sessionTokenResult = httpContext.GetAccessSessionToken();

            if (!sessionTokenResult.Ok || !await accessService.HasPermissionAsync(sessionTokenResult.SessionToken, "recall://default/events"))
            {
                return Results.Ok(new Models.EventStoreResponse<EventType>());
            }

            var specification = new EventType.Specification();

            if (model.MaximumRows > 0)
            {
                specification.WithMaximumRows(model.MaximumRows);
            }

            if (!string.IsNullOrEmpty(model.TypeNameMatch))
            {
                specification.WithTypeNameMatch(model.TypeNameMatch);
            }
            
            List<EventType> result;

            using (new DatabaseContextScope())
            await using (databaseContextFactory.Create())
            {
                result = (await eventTypeQuery.SearchAsync(specification)).ToList();
            }

            return Results.Ok(new Models.EventStoreResponse<EventType>
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