using Dapper;
using FirebirdSql.Data.FirebirdClient;
using Npgsql;
using Sql2Excel.Model.Entities;
using Sql2Excel.Model.Enums;
using Sql2Excel.Utils;
using System.Data;

namespace Sql2Excel.Services;

public class DatabaseService
{

    public static async Task<IEnumerable<dynamic>> QueryData(IDbConnection conn, string sql)
    {
        if (conn.State != ConnectionState.Open)
            conn.Open();

        using var transaction = conn.BeginTransaction();
        try
        {
            return await conn.QueryAsync(sql, transaction: transaction);
        }
        finally
        {
            transaction.Rollback();
        }
    }

    public static IDbConnection? GetDbConnection(ExecutionParameters executionParameters)
    {
        if (string.IsNullOrEmpty(executionParameters.GetConnectionString()))
        {
            NotificationUtil.ShowError("ConnectionString must not be null");
            return null;
        }

        switch (executionParameters.GetDatabaseDriver())
        {
            case DatabaseDriver.FIREBIRD:
                return new FbConnection(executionParameters.GetConnectionString());
            case DatabaseDriver.POSTGRES:
                return new NpgsqlConnection(executionParameters.GetConnectionString());
            default:
                NotificationUtil.ShowError("Database not supported");
                return null;
        }
    }
}
