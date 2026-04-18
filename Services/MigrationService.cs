using Dapper;
using DbMigrator.Interfaces;
using DbMigrator.Models;
using System.Data;

namespace DbMigrator.Services;

public class MigrationService(IDbConnectionFactory dbConnectionFactory) : IDbMigrator
{
    public async Task MigrateAsync(string sourceConnectionString, string targetConnectionString, CancellationToken cancellationToken = default)
    {
        using var sourceConn = dbConnectionFactory.CreateConnection(sourceConnectionString);
        using var targetConn = dbConnectionFactory.CreateConnection(targetConnectionString);

        var tablesInfo = await sourceConn.QueryAsync<TableInfo>(
            """
                SELECT TABLE_SCHEMA [Schema], TABLE_NAME [Name]
                FROM INFORMATION_SCHEMA.TABLES
                WHERE TABLE_TYPE = 'BASE TABLE' AND TABLE_NAME NOT LIKE '%migration%' 
            """, cancellationToken);

        /*
        PrepareEnvironment(tablesInfo, targetConn);
    */
    }

    /*
    private void PrepareEnvironment(IEnumerable<TableInfo> tablesInfo, IDbConnection targetConn)
    {
        var availableSchemas = tablesInfo.Select(t => t.Schema).Distinct();
        var targetSchemas = targetConn.Query<string>("SELECT SCHEMA_NAME FROM INFORMATION_SCHEMA.SCHEMATA");

        var missingSchemas = availableSchemas.Except(targetSchemas);
        if (missingSchemas.Any())
        {
            foreach (var schema in missingSchemas)
            {
                targetConn.Execute($"CREATE SCHEMA {schema}");
            }
        }

        var availableTables = tablesInfo.Select(t => t.Name).Distinct();
        var targetTables = targetConn.Query<string>(
            """
                SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES
                WHERE TABLE_TYPE = 'BASE TABLE' AND TABLE_NAME NOT LIKE '%migration%'
            """);


        // TODO: Complete the Table Preperation logic
        var missingTables = availableTables.Except(targetTables);
        if (missingTables.Any())
        {
            foreach (var table in missingTables)
            {
                targetConn.Execute($"CREATE TABLE {table} (Id INT IDENTITY PRIMARY KEY)");
            }
        }
    }
*/
}