using System.Data;
using Asp.Versioning;
using Asp.Versioning.Builder;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Shuttle.Access.AspNetCore;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Recall.SqlServer.Storage;
using Shuttle.Recall.WebApi.Models;
using EventType = Shuttle.Recall.WebApi.Models.EventType;

namespace Shuttle.Recall.WebApi;

public static class EventTypeEndpoints
{
    public static WebApplication MapEventTypeEndpoints(this WebApplication app, ApiVersionSet versionSet)
    {
        var apiVersion1 = new ApiVersion(1, 0);

        app.MapPost("/event-types/search", async (IOptions<SqlServerStorageOptions> sqlServerStorageOptions, ISessionContext sessionContext, SqlServerStorageDbContext dbContext, EventType.Specification specification, CancellationToken cancellationToken) =>
            {
                Guard.AgainstNull(sessionContext);
                Guard.AgainstNull(dbContext);

                if (!(sessionContext.Session?.HasPermission("recall://default/events") ?? false))
                {
                    return Results.Ok(new EventStoreResponse<EventType>());
                }

                var connection = dbContext.Database.GetDbConnection();

                await using var command = connection.CreateCommand();

                command.CommandText = $@"
SELECT {(specification.MaximumRows > 0 ? $"TOP {specification.MaximumRows}" : string.Empty)}
    Id,
    TypeName
FROM
    [{sqlServerStorageOptions.Value.Schema}].[EventType]
WHERE
(
    @TypeNameMatch IS NULL
    OR
    TypeName LIKE '%' + @TypeNameMatch + '%'
)
";

                command.Parameters.Add(new SqlParameter("@TypeNameMatch", specification.TypeNameMatch));

                if (connection.State != ConnectionState.Open)
                {
                    await connection.OpenAsync(cancellationToken);
                }

                List<EventType> result = [];

                await using var reader = await command.ExecuteReaderAsync(cancellationToken);

                while (await reader.ReadAsync(cancellationToken))
                {
                    result.Add(new()
                    {
                        Id = reader.GetGuid(0),
                        TypeName = reader.GetString(1)
                    });
                }

                return Results.Ok(new EventStoreResponse<EventType>
                {
                    Items = result
                });
            })
            .WithTags("EventTypes")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1);

        return app;
    }
}