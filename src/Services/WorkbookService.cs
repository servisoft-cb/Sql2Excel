
using ClosedXML.Excel;
using Sql2Excel.Model.Extensions;
using Sql2Excel.Utils;
using System.Data;
using System.IO;
using System.Windows;

namespace Sql2Excel.Services;

public static class WorkbookService
{
    public static XLWorkbook GenerateXlsx(IEnumerable<dynamic> data, XLTableTheme theme)
    {
        var xlWorkbook = new XLWorkbook();
        var ws = xlWorkbook.AddWorksheet("Result");

        var table = ws.Cell(1, 1).InsertTable(data.ToDataTable());

        table.Theme = theme;
        ws.Columns().AdjustToContents();

        return xlWorkbook;
    }

    public static void Persist(XLWorkbook workbook, string outputPath, string? filename = "result.xlsx")
    {
        if (!Path.Exists(outputPath))
        {
            NotificationUtil.ShowError("Output path not found");
            return;
        }

        if (!Path.HasExtension($"{outputPath}\\{filename}"))
        {
            NotificationUtil.ShowError("filename do not have an extension");
            return;
        }

        workbook.SaveAs($"{outputPath}\\{filename}");
    }



}
