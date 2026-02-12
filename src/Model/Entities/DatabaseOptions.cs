using Sql2Excel.Model.Enums;

namespace Sql2Excel.Model.Entities;

public sealed class DatabaseOptions
{
    public string ConnectionString { get; set; } = string.Empty;
    public DatabaseDriver Driver { get; set; }
}
