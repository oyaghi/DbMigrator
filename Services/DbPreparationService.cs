using Dapper;
using DbMigrator.Interfaces;
using DbMigrator.Models;
using System.Data;

namespace DbMigrator.Services;

public class DbPreparationService(IDbConnectionFactory dbConnectionFactory) : IDbPreparationService
{
    public async Task<bool> PrepareTargetDbAsync(List<TableInfo> sourceInfo, string targetConnectionString, CancellationToken cancellationToken = default)
    {
        using var targetConn = dbConnectionFactory.CreateConnection(targetConnectionString);
        targetConn.Open();

        await ToggleConstraintsAsync(targetConn, sourceInfo, false);
        await ClearTablesAsync(targetConn, sourceInfo);

        return true;
    }

    private static async Task ClearTablesAsync(IDbConnection targetConn, List<TableInfo> tablesInfo)
    {
        foreach (var tableInfo in tablesInfo)
        {
            await targetConn.ExecuteAsync($"DELETE FROM [{tableInfo.Schema}].[{tableInfo.Name}]");
        }
    }

    public async Task ToggleConstraintsAsync(IDbConnection conn, List<TableInfo> tablesInfo, bool enable)
    {
        var constraintCommand = enable ? "CHECK" : "NOCHECK";
        var triggerCommand = enable ? "ENABLE" : "DISABLE";

        foreach (var tableInfo in tablesInfo)
        {
            // Toggle Foreign Key Constraints
            await conn.ExecuteAsync($"ALTER TABLE [{tableInfo.Schema}].[{tableInfo.Name}] {constraintCommand} CONSTRAINT ALL");

            // Toggle Triggers
            await conn.ExecuteAsync($"{triggerCommand} TRIGGER ALL ON [{tableInfo.Schema}].[{tableInfo.Name}]");
        }
    }
}