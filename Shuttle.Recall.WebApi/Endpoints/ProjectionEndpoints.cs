using System.Data;
using Asp.Versioning;
using Asp.Versioning.Builder;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Shuttle.Access.AspNetCore;
using Shuttle.Contract;
using Shuttle.Recall.SqlServer.Storage;
using Shuttle.Recall.WebApi.Contracts.v1;

namespace Shuttle.Recall.WebApi;

public static class ProjectionEndpoints
{
    public static WebApplication MapProjectionEndpoints(this WebApplication app, ApiVersionSet versionSet)
    {
        var apiVersion1 = new ApiVersion(1, 0);

        app.MapPost("/projections/search", PostSearch)
            .WithTags("Projections")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1);

        app.MapPost("/projections", PostSave)
            .WithTags("Projections")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1);

        return app;
    }

    private static async Task<IResult> PostSearch(ISqlServerStorageSchemaAccessor schemaAccessor, ISessionContext sessionContext, IEventStoreContext eventStoreContext, SqlServerStorageDbContext dbContext, Contracts.v1.Projection.Specification specification, CancellationToken cancellationToken)
    {
        Guard.AgainstNull(sessionContext);
        Guard.AgainstNull(eventStoreContext);
        Guard.AgainstNull(schemaAccessor);
        Guard.AgainstNull(dbContext);

        if (!eventStoreContext.HasAccess(sessionContext))
        {
            return Results.Ok(new EventStoreResponse<Contracts.v1.Projection>());
        }

        var exception = eventStoreContext.ValidateEventStore();

        if (exception != null)
        {
            return Results.Ok(new EventStoreResponse<Contracts.v1.Projection> { Exception = exception });
        }

        dbContext.Database.SetConnectionString(eventStoreContext.EventStore.ConnectionString);
        schemaAccessor.Schema = eventStoreContext.EventStore.Schema;

        var connection = dbContext.Database.GetDbConnection();

        await using var command = connection.CreateCommand();

        command.CommandText = $@"
SELECT {(specification.MaximumRows > 0 ? $"TOP {specification.MaximumRows}" : string.Empty)}
    [Name],
    [SequenceNumber],
    [FailureCount],
    [DeferredUntil]
FROM
    [{schemaAccessor.Schema}].[Projection]
WHERE
    [Name] LIKE '%' + @NameMatch + '%'
AND
    [FailureCount] >= @FailureCountStart
AND
    [SequenceNumber] >= @SequenceNumberStart
AND
(
    @Deferred IS NULL
    OR
    (@Deferred = 1 AND [DeferredUntil] IS NOT NULL)
    OR
    (@Deferred = 0 AND [DeferredUntil] IS NULL)
)
ORDER BY
    [Name]
";

        command.Parameters.Add(new SqlParameter("@NameMatch", specification.NameMatch));
        command.Parameters.Add(new SqlParameter("@FailureCountStart", specification.FailureCountStart));
        command.Parameters.Add(new SqlParameter("@SequenceNumberStart", specification.SequenceNumberStart));
        command.Parameters.Add(new SqlParameter("@Deferred", (object?)specification.Deferred ?? DBNull.Value));

        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken);
        }

        List<Contracts.v1.Projection> result = [];

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            result.Add(new()
            {
                Name = reader.GetString(0),
                SequenceNumber = reader.GetInt64(1),
                FailureCount = reader.GetInt32(2),
                DeferredUntil = reader.IsDBNull(3) ? null : reader.GetFieldValue<DateTimeOffset>(3)
            });
        }

        return Results.Ok(new EventStoreResponse<Contracts.v1.Projection>
        {
            Items = result
        });
    }

    private static async Task<IResult> PostSave(ISqlServerStorageSchemaAccessor schemaAccessor, ISessionContext sessionContext, IEventStoreContext eventStoreContext, SqlServerStorageDbContext dbContext, Contracts.v1.Projection model, CancellationToken cancellationToken)
    {
        Guard.AgainstNull(sessionContext);
        Guard.AgainstNull(eventStoreContext);
        Guard.AgainstNull(schemaAccessor);
        Guard.AgainstNull(dbContext);

        if (!eventStoreContext.HasAccess(sessionContext))
        {
            return Results.Ok(new EventStoreResponse<Contracts.v1.Projection>());
        }

        var exception = eventStoreContext.ValidateEventStore();

        if (exception != null)
        {
            return Results.Ok(new EventStoreResponse<Contracts.v1.Projection> { Exception = exception });
        }

        if (string.IsNullOrWhiteSpace(model.Name))
        {
            return Results.BadRequest("No projection name has been specified.");
        }

        if (model.SequenceNumber < 0)
        {
            return Results.BadRequest("The sequence number may not be a negative value.");
        }

        if (model.FailureCount < 0)
        {
            return Results.BadRequest("The failure count may not be a negative value.");
        }

        dbContext.Database.SetConnectionString(eventStoreContext.EventStore.ConnectionString);
        schemaAccessor.Schema = eventStoreContext.EventStore.Schema;

        var connection = dbContext.Database.GetDbConnection();

        await using var command = connection.CreateCommand();

        command.CommandText = $@"
UPDATE
    [{schemaAccessor.Schema}].[Projection]
SET
    [SequenceNumber] = @SequenceNumber,
    [FailureCount] = @FailureCount,
    [DeferredUntil] = @DeferredUntil
WHERE
    [Name] = @Name
";

        command.Parameters.Add(new SqlParameter("@Name", model.Name));
        command.Parameters.Add(new SqlParameter("@SequenceNumber", model.SequenceNumber));
        command.Parameters.Add(new SqlParameter("@FailureCount", model.FailureCount));
        command.Parameters.Add(new SqlParameter("@DeferredUntil", (object?)model.DeferredUntil ?? DBNull.Value));

        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken);
        }

        var rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);

        if (rowsAffected == 0)
        {
            return Results.NotFound($"No projection named '{model.Name}' could be found.");
        }

        return Results.Ok();
    }
}
