using Microsoft.Extensions.Configuration;
using Sql2Excel.Model.Entities;
using Sql2Excel.Model.Enums;
using System.IO;
using System.Text;

namespace MailTool.Config;

public class ConfigLoader
{
    public IConfiguration Config { get; private set; } = default!;

    public ConfigLoader()
    {
        LoadConfig();
    }

    public string GetConnectionString(string section = "SSFacil")
    {
        var database = Config[$"{section}:database"];
        var username = Config[$"{section}:username"];

        var sb = new StringBuilder();
        sb.Append($"User={username};");
        sb.Append($"Password=masterkey;");
        sb.Append($"Database={database};");

        return sb.ToString();
    }

    public ExecutionParameters GetExecutionParams()
    {
        return new ExecutionParameters
        {
            DatabaseOptions = new DatabaseOptions
            {
                ConnectionString = this.GetConnectionString(),
                Driver = DatabaseDriver.FIREBIRD
            },
            WorkbookTheme = int.Parse(Config["SSFacil:temaPadraoExcel"])
        };
    }

    private void LoadConfig()
    {
        var projectRoot = Directory.GetCurrentDirectory();
        var iniPath = Path.Combine(projectRoot, "config.ini");

        if (!File.Exists(iniPath))
            throw new FileNotFoundException("Arquivo config.ini não encontrado na raiz do projeto.", iniPath);

        var processedLines = PreprocessIni(File.ReadAllLines(iniPath));

        var iniContent = string.Join(Environment.NewLine, processedLines);
        using var ms = new MemoryStream(Encoding.UTF8.GetBytes(iniContent));

        Config = new ConfigurationBuilder()
            .AddIniStream(ms)
            .Build();
    }

    private IEnumerable<string> PreprocessIni(string[] lines)
    {
        var seenKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var result = new List<string>();
        string? currentSection = null;

        foreach (var raw in lines)
        {
            var line = raw.Trim();

            if (IsCommentOrEmpty(line))
            {
                result.Add(raw);
                continue;
            }

            if (IsSection(line))
            {
                currentSection = line.Trim('[', ']');
                result.Add(raw);
                seenKeys.Clear();
                continue;
            }

            if (IsKeyValue(line))
            {
                HandleKeyValue(raw, line, currentSection, result, seenKeys);
                continue;
            }

            if (line.StartsWith("*"))
            {
                result.Add(";" + raw);
                continue;
            }

            // 👉 Caso especial: linha sem "=" dentro de seção → vira flag
            if (currentSection is not null && !string.IsNullOrWhiteSpace(line))
            {
                var key = line;
                var compositeKey = $"{currentSection}:{key}";

                if (!seenKeys.Contains(compositeKey))
                {
                    seenKeys.Add(compositeKey);
                    result.Add($"{key}=true");
                }
                else
                {
                    // se já existir, substitui pela última ocorrência
                    var idx = result.FindIndex(l => l.StartsWith(key + "=", StringComparison.OrdinalIgnoreCase));
                    if (idx >= 0) result[idx] = $"{key}=true";
                }
                continue;
            }

            // fallback: comenta para não quebrar o parser
            result.Add(";" + raw);
        }

        return result;
    }


    private static bool IsCommentOrEmpty(string line) =>
        string.IsNullOrWhiteSpace(line) || line.StartsWith(";") || line.StartsWith("#");

    private static bool IsSection(string line) =>
        line.StartsWith("[") && line.EndsWith("]");

    private static bool IsKeyValue(string line) =>
        line.Contains("=");

    private static void HandleKeyValue(
        string raw, string line, string? section,
        List<string> result, HashSet<string> seenKeys)
    {
        var key = line.Split('=')[0].Trim();
        var compositeKey = section is null ? key : $"{section}:{key}";

        if (seenKeys.Contains(compositeKey))
        {
            // Substitui valor anterior mantendo só o último
            var idx = result.FindIndex(l => l.TrimStart().StartsWith(key + "=", StringComparison.OrdinalIgnoreCase));
            if (idx >= 0) result[idx] = raw;
        }
        else
        {
            seenKeys.Add(compositeKey);
            result.Add(raw);
        }
    }
}
