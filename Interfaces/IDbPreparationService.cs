using DbMigrator.Models;

namespace DbMigrator.Interfaces;

public interface IDbPreparationService
{
    Task<bool> PrepareTargetDbAsync(List<TableInfo> sourceInfo, string targetConnectionString, bool isMigrationApplied = false, CancellationToken cancellationToken = default);
}