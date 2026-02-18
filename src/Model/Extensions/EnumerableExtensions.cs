using System.Data;

namespace Sql2Excel.Model.Extensions;

public static class EnumerableExtensions
{
    public static DataTable ToDataTable(this IEnumerable<dynamic> data)
    {
        var dataTable = new DataTable();

        var firstRow = data.FirstOrDefault() as IDictionary<string, object>;

        if (firstRow == null)
            return dataTable;

        foreach (var kvp in firstRow)
        {

            if (dataTable.Columns.Contains(kvp.Key))
            {
                continue;
            }

            var columnType = kvp.Value?.GetType() ?? typeof(object);
            dataTable.Columns.Add(kvp.Key, columnType);
        }

        foreach (IDictionary<string, object> row in data)
        {
            var newRow = dataTable.NewRow();
            foreach (var kvp in row)
            {
                newRow[kvp.Key] = kvp.Value ?? DBNull.Value;
            }
            dataTable.Rows.Add(newRow);
        }

        return dataTable;
    }
}
