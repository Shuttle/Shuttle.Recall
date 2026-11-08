using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Shuttle.Contract;
using Shuttle.Recall.SqlServer.Storage;
using Shuttle.Recall.WebApi.Contracts.v1;

namespace Shuttle.Recall.WebApi;

[SuppressMessage("Security", "EF1002:Risk of vulnerability to SQL injection", Justification = "Schema and table names are from trusted configuration sources")]
public class EventStoreRebuilder(ISqlServerStorageSchemaAccessor schemaAccessor, SqlServerStorageDbContext dbContext) : IEventStoreRebuilder
{
    public async Task<Rebuild.Result> RebuildAsync(ApiOptions.EventStoreOptions eventStore, Rebuild rebuild, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(eventStore);
        Guard.AgainstNull(rebuild);

        var schema = schemaAccessor.Schema;
        var result = new Rebuild.Result();

        var connection = dbContext.Database.GetDbConnection();

        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken);
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

        var dbTransaction = transaction.GetDbTransaction();

        if (rebuild.SequenceNumberFromInclusive.HasValue)
        {
            await using var command = CreateCommand(connection, dbTransaction, $@"
DECLARE @Removed TABLE ([Id] UNIQUEIDENTIFIER NOT NULL, [EventId] UNIQUEIDENTIFIER NOT NULL);
DECLARE @EventsRemoved INT;
DECLARE @IdKeysRemoved INT;

DELETE FROM
    [{schema}].[PrimitiveEvent]
OUTPUT
    DELETED.[Id], DELETED.[EventId] INTO @Removed
WHERE
    [SequenceNumber] >= @SequenceNumberFromInclusive;

SET @EventsRemoved = @@ROWCOUNT;

-- unique keys belonging to aggregates that no longer have any events are released
DELETE
    ik
FROM
    [{schema}].[IdKey] ik
WHERE
    ik.[Id] IN (SELECT [Id] FROM @Removed)
AND
    NOT EXISTS (SELECT NULL FROM [{schema}].[PrimitiveEvent] pe WHERE pe.[Id] = ik.[Id]);

SET @IdKeysRemoved = @@ROWCOUNT;

IF OBJECT_ID(N'[{schema}].[ImmediateProjectionEvent]', N'U') IS NOT NULL
BEGIN
    DELETE FROM
        [{schema}].[ImmediateProjectionEvent]
    WHERE
        [EventId] IN (SELECT [EventId] FROM @Removed);
END

-- projections that are not being reset may not skip any events that are sequenced after the removal
UPDATE
    [{schema}].[Projection]
SET
    [SequenceNumber] = @SequenceNumberFromInclusive - 1
WHERE
    [SequenceNumber] >= @SequenceNumberFromInclusive;

SELECT @EventsRemoved, @IdKeysRemoved;
");

            command.Parameters.Add(new SqlParameter("@SequenceNumberFromInclusive", rebuild.SequenceNumberFromInclusive.Value));

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            if (await reader.ReadAsync(cancellationToken))
            {
                result.EventsRemoved = reader.GetInt32(0);
                result.IdKeysRemoved = reader.GetInt32(1);
            }
        }

        foreach (var projectionTable in eventStore.ProjectionTables.Where(configured => rebuild.ProjectionTables.Contains(configured, StringComparer.OrdinalIgnoreCase)))
        {
            await using var command = CreateCommand(connection, dbTransaction, $"DELETE FROM {GetQuotedTableName(projectionTable)};");

            result.ProjectionTableRowsRemoved[projectionTable] = await command.ExecuteNonQueryAsync(cancellationToken);
        }

        if (rebuild.ProjectionNames.Count > 0)
        {
            var parameterNames = rebuild.ProjectionNames.Select((_, index) => $"@Name{index}").ToList();

            await using var command = CreateCommand(connection, dbTransaction, $@"
DECLARE @ProjectionsReset INT;

UPDATE
    [{schema}].[Projection]
SET
    [SequenceNumber] = 0,
    [FailureCount] = 0,
    [DeferredUntil] = NULL,
    [LockedAt] = NULL
WHERE
    [Name] IN ({string.Join(", ", parameterNames)});

SET @ProjectionsReset = @@ROWCOUNT;

-- events handled immediately would otherwise be skipped when the projection is processed again
IF OBJECT_ID(N'[{schema}].[ImmediateProjectionEvent]', N'U') IS NOT NULL
BEGIN
    DELETE FROM
        [{schema}].[ImmediateProjectionEvent]
    WHERE
        [ProjectionName] IN ({string.Join(", ", parameterNames)});
END

SELECT @ProjectionsReset;
");

            for (var index = 0; index < rebuild.ProjectionNames.Count; index++)
            {
                command.Parameters.Add(new SqlParameter(parameterNames[index], rebuild.ProjectionNames[index]));
            }

            result.ProjectionsReset = Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken));
        }

        await transaction.CommitAsync(cancellationToken);

        return result;
    }

    private static DbCommand CreateCommand(DbConnection connection, DbTransaction transaction, string commandText)
    {
        var command = connection.CreateCommand();

        command.Transaction = transaction;
        command.CommandText = commandText;

        return command;
    }

    private static string GetQuotedTableName(string tableName)
    {
        var parts = Guard.AgainstEmpty(tableName)
            .Split('.', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Select(part => $"[{part.TrimStart('[').TrimEnd(']').Replace("]", "]]")}]")
            .ToList();

        if (parts.Count is < 1 or > 3)
        {
            throw new InvalidOperationException($"Projection table name '{tableName}' is not valid.");
        }

        return string.Join(".", parts);
    }
}
