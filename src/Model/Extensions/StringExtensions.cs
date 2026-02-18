using System.Text.RegularExpressions;

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

    public static IEnumerable<string> CorrigirSqlParaFirebird(this string sql)
    {
        // Regex para encontrar o padrão "CAMPO IN (1,2,3...)"
        var regex = new Regex(@"(\w+\.?\w+)\s+IN\s*\(([^)]+)\)", RegexOptions.IgnoreCase);

        var match = regex.Match(sql);

        // Se não encontrar IN com lista de IDs, retorna o SQL original
        if (!match.Success)
        {
            yield return sql;
            yield break;
        }

        string nomeCampo = match.Groups[1].Value;
        string todosIdsRaw = match.Groups[2].Value;

        var listaIds = todosIdsRaw.Split(',')
                                  .Select(id => id.Trim())
                                  .Where(id => !string.IsNullOrWhiteSpace(id))
                                  .ToList();

        if (listaIds.Count <= 1000)
        {
            yield return sql;
            yield break;
        }

        // Quebra em blocos de 10.000 IDs (cada bloco vira um SELECT separado)
        for (int blocoInicio = 0; blocoInicio < listaIds.Count; blocoInicio += 10000)
        {
            var idsDoBlocoGrande = listaIds.Skip(blocoInicio).Take(10000).ToList();

            // Dentro de cada bloco de 20k, quebra em ORs de 1000
            var blocosOr = new List<string>();
            for (int i = 0; i < idsDoBlocoGrande.Count; i += 1000)
            {
                var chunk = idsDoBlocoGrande.Skip(i).Take(1000);
                blocosOr.Add($"{nomeCampo} IN ({string.Join(",", chunk)})");
            }

            // Substitui a cláusula IN original pela versão com ORs
            string whereClause = "(" + string.Join(" OR ", blocosOr) + ")";
            string sqlDoBlocoAtual = regex.Replace(sql, whereClause, 1);

            yield return sqlDoBlocoAtual;
        }
    }

}
