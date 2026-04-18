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
    }

    /*
    private async Task<IEnumerable<TableInfo>> GetTablesInfoAsync(IDbConnection connection, CancellationToken cancellationToken = default) { }
*/
}