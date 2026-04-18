namespace DbMigrator.Interfaces;

public interface IDbMigrator
{
    Task MigrateAsync(string sourceConnectionString, string targetConnectionString, CancellationToken cancellationToken = default);
}