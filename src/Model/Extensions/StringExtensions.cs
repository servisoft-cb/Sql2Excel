using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Media.Animation;

namespace Sql2Excel.Model.Extensions;

public static class StringExtensions
{

    public static bool IsValidSqlQuery(this string sql)
    {
        if (!sql.TrimStart().StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
            return false;

        if (sql.Contains(';'))
            return false;

        string blacklist = @"\b(INSERT|UPDATE|DELETE|DROP|TRUNCATE|ALTER|CREATE|EXEC|EXECUTE|REPLACE|MERGE)\b";
        if (Regex.IsMatch(sql, blacklist, RegexOptions.IgnoreCase))
            return false;

        return true;
    }

}
