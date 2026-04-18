using System.ComponentModel.DataAnnotations.Schema;

namespace DbMigrator.Models;

public class TableInfo
{
    public string Name { get; set; } = null!;

    public string Schema { get; set; } = null!;
}