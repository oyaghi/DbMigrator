using Dapper;
using DbMigrator.Interfaces;
using DbMigrator.Models;
using System.Data;

namespace DbMigrator.Services;

public class MigrationService(
    IDbConnectionFactory dbConnectionFactory,
    IDbPreparationService dbPreparationService) : IDbMigrator
{
    public async Task MigrateAsync(string sourceConnectionString, string targetConnectionString, CancellationToken cancellationToken = default)
    {
        using var sourceConn = dbConnectionFactory.CreateConnection(sourceConnectionString);
        // We open the target connection here to keep it alive for the Toggle and the migration
        using var targetConn = dbConnectionFactory.CreateConnection(targetConnectionString);
        targetConn.Open();

        var sourceTablesInfo = await sourceConn.QueryAsync<TableInfo>(new CommandDefinition(
            """
            SELECT TABLE_SCHEMA AS [Schema], TABLE_NAME AS [Name]
            FROM INFORMATION_SCHEMA.TABLES
            WHERE TABLE_TYPE = 'BASE TABLE' AND TABLE_NAME NOT LIKE '%migration%' 
            """, cancellationToken: cancellationToken));

        var tablesList = sourceTablesInfo.ToList();

        // 1. Prepare (Clean tables, Disable constraints)
        await dbPreparationService.PrepareTargetDbAsync(tablesList, targetConnectionString, cancellationToken: cancellationToken);

        // 2. Migrate Data
        foreach (var tableInfo in tablesList)
        {
            await MigrateTableAsync(tableInfo, sourceConn, targetConn, cancellationToken);
        }

        // 3. Cleanup (Re-enable constraints)
        await dbPreparationService.ToggleConstraintsAsync(targetConn, tablesList, true);
    }

    private static async Task MigrateTableAsync(TableInfo tableInfo, IDbConnection sourceConn, IDbConnection targetConn, CancellationToken cancellationToken)
    {
        var batchSize = 1000;
        int totalMigrated = 0;
        bool hasMoreData = true;

        // Get columns to build a dynamic INSERT statement
        var columns = await sourceConn.QueryAsync<string>(
            "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = @Name AND TABLE_SCHEMA = @Schema",
            new { tableInfo.Name, tableInfo.Schema });

        var columnList = columns.ToList();
        if (!columnList.Any()) return;

        // Build INSERT INTO [Schema].[Table] (Col1, Col2) VALUES (@Col1, @Col2)
        var columnNames = string.Join(", ", columnList.Select(c => $"[{c}]"));
        var paramNames = string.Join(", ", columnList.Select(c => $"@{c}"));
        var insertSql = $"INSERT INTO [{tableInfo.Schema}].[{tableInfo.Name}] ({columnNames}) VALUES ({paramNames})";

        Console.WriteLine($"Starting migration for {tableInfo.Schema}.{tableInfo.Name}...");

        // Handle Identity Insert if needed (SQL Server specific)
        await targetConn.ExecuteAsync($"SET IDENTITY_INSERT [{tableInfo.Schema}].[{tableInfo.Name}] ON").CatchIgnore();

        while (hasMoreData)
        {
            // Fetch batch using OFFSET/FETCH
            var rows = await sourceConn.QueryAsync<dynamic>(new CommandDefinition(
                $@"SELECT * FROM [{tableInfo.Schema}].[{tableInfo.Name}] 
                   ORDER BY (SELECT NULL) 
                   OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY",
                new { Skip = totalMigrated, Take = batchSize },
                cancellationToken: cancellationToken));

            var batchList = rows.ToList();

            if (batchList.Count > 0)
            {
                await targetConn.ExecuteAsync(insertSql, batchList);
                totalMigrated += batchList.Count;
                Console.WriteLine($"-- {tableInfo.Name}: Migrated {totalMigrated} rows...");
            }
            else
            {
                hasMoreData = false;
            }
        }

        await targetConn.ExecuteAsync($"SET IDENTITY_INSERT [{tableInfo.Schema}].[{tableInfo.Name}] OFF").CatchIgnore();
    }
}

// Simple extension to ignore errors on tables that don't have Identity columns
public static class DbExtensions
{
    public static async Task CatchIgnore(this Task task) {
        try { await task; } catch { /* Identity Insert probably wasn't needed */ }
    }
}