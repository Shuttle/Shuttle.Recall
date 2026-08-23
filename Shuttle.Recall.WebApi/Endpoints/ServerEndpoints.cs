using System.Reflection;
using Asp.Versioning;
using Asp.Versioning.Builder;
using Microsoft.Extensions.Options;
using Shuttle.Access.AspNetCore;
using Shuttle.Recall.WebApi.Models;

namespace Shuttle.Recall.WebApi;

public static class ServerEndpoints
{
    public static WebApplication MapServerEndpoints(this WebApplication app, ApiVersionSet versionSet)
    {
        var apiVersion1 = new ApiVersion(1, 0);

        app.MapGet("/v{version:apiVersion}/server/configuration", () =>
            {
                var version = Assembly.GetExecutingAssembly().GetName().Version ?? new Version(0, 0, 0);

                return Results.Ok(new { Version = $"{version.Major}.{version.Minor}.{version.Build}" });
            })
            .WithTags("Server")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1);

        app.MapGet("/v{version:apiVersion}/server/event-stores", (IOptions<ApiOptions> apiOptions, ISessionContext sessionContext) =>
            {
                var result = apiOptions.Value.EventStores.Select(eventStore => new EventStoreInfo
                {
                    Name = eventStore.Name,
                    HasAccess = string.IsNullOrWhiteSpace(eventStore.Permission) || sessionContext.HasPermission(eventStore.Permission)
                }).ToList();

                return Results.Ok(new EventStoreResponse<EventStoreInfo> { Items = result });
            })
            .WithTags("Server")
            .RequireSession()
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1);

        return app;
    }
}