using Dapper;
using DbMigrator.Interfaces;
using DbMigrator.Models;
using System.Data;

namespace DbMigrator.Services;

public class DbPreparationService(IDbConnectionFactory dbConnectionFactory) : IDbPreparationService
{
    public async Task<bool> PrepareTargetDbAsync(List<TableInfo> sourceInfo, string targetConnectionString, bool isMigrationApplied = false, CancellationToken cancellationToken = default)
    {
        var targetConn = dbConnectionFactory.CreateConnection(targetConnectionString);

        return true;
    }

    private async Task PrepareSchemasAsync(IDbConnection targetConn, List<string> sourceSchemas)
    {

    }

    private async Task PrepareTablesAsync(IDbConnection targetConn, List<string> sourceTables)
    {

    }

    private async Task ClearTablesAsync(IDbConnection targetConn, List<string> sourceTables)
    {

    }

    private async Task ToggleConstraintsAsync(IDbConnection conn, TableInfo table, bool enable)
    {
        var constraintCommand = enable ? "CHECK" : "NOCHECK";
        var triggerCommand = enable ? "ENABLE" : "DISABLE";

        // Toggle Foreign Key Constraints
        await conn.ExecuteAsync($"ALTER TABLE [{table.Schema}].[{table.Name}] {constraintCommand} CONSTRAINT ALL");

        // Toggle Triggers
        await conn.ExecuteAsync($"{triggerCommand} TRIGGER ALL ON [{table.Schema}].[{table.Name}]");

        /*
        Console.WriteLine($"{(enable ? "Enabled" : "Disabled")} constraints/triggers for {table.Name}");
    */
    }}