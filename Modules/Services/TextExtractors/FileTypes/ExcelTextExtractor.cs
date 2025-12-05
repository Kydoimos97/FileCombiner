using System.Text;
using ClosedXML.Excel;

namespace FileCombiner.Modules.Services.TextExtractors.FileTypes;

public class ExcelTextExtractor : IFileTextExtractor
{
    public bool CanHandle(string extension)
    {
        return extension is ".xlsx" or ".xls";
    }

    public async Task<string> ExtractTextAsync(string path)
    {
        return await Task.Run(() =>
        {
            var sb = new StringBuilder();
            using var workbook = new XLWorkbook(path);
            foreach (var ws in workbook.Worksheets)
            {
                sb.AppendLine($"# Sheet: {ws.Name}");
                foreach (var row in ws.RowsUsed())
                {
                    var values = row.CellsUsed().Select(c => c.GetValue<string>());
                    sb.AppendLine(string.Join("\t", values));
                }

                sb.AppendLine();
            }

            return sb.ToString();
        });
    }
}