using DbMigrator.Models;

namespace DbMigrator.Interfaces;

public interface IDbPreparationService
{
    Task<bool> PrepareTargetDbAsync(List<TableInfo> sourceInfo, string targetConnectionString, CancellationToken cancellationToken = default);
}