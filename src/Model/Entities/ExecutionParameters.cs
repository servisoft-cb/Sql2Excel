using ClosedXML.Excel;
using Sql2Excel.Model.Enums;

namespace Sql2Excel.Model.Entities;

public class ExecutionParameters
{
    public DatabaseOptions DatabaseOptions { get; set; } = default!;
    public string DestinationPath { get; set; } = string.Empty;
    private int WorkbookTheme { get; set; } = 1;

    public ExecutionParameters() { }


    public string? GetConnectionString()
    {
        return DatabaseOptions.ConnectionString;
    }

    public XLTableTheme GetWorkbookTheme()
    {
        return WorkbookTheme switch
        {
            1 => XLTableTheme.TableStyleMedium1,
            2 => XLTableTheme.TableStyleMedium2,
            3 => XLTableTheme.TableStyleMedium9,
            4 => XLTableTheme.TableStyleMedium10,
            5 => XLTableTheme.TableStyleMedium11,
            6 => XLTableTheme.TableStyleMedium12,
            7 => XLTableTheme.TableStyleMedium14,
            8 => XLTableTheme.TableStyleLight1,
            9 => XLTableTheme.TableStyleLight8,
            10 => XLTableTheme.TableStyleDark1,
            _ => XLTableTheme.TableStyleMedium1
        };
    }
    
    public DatabaseDriver GetDatabaseDriver()
    {
        return DatabaseOptions.Driver;
    }
}
