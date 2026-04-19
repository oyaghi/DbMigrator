using DbMigrator.Models;
using System.Data;

namespace DbMigrator.Interfaces;

public interface IDbPreparationService
{
    Task<bool> PrepareTargetDbAsync(List<TableInfo> sourceInfo, string targetConnectionString, CancellationToken cancellationToken = default);

    Task ToggleConstraintsAsync(IDbConnection conn, List<TableInfo> tablesInfo, bool enable);
}