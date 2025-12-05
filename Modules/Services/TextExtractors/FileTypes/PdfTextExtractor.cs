using System.Text;
using UglyToad.PdfPig;

namespace FileCombiner.Modules.Services.TextExtractors.FileTypes;

public class PdfTextExtractor : IFileTextExtractor
{
    public bool CanHandle(string extension)
    {
        return extension.Equals(".pdf", StringComparison.OrdinalIgnoreCase);
    }

    public async Task<string> ExtractTextAsync(string path)
    {
        return await Task.Run(() =>
        {
            try
            {
                var sb = new StringBuilder();
                using var pdf = PdfDocument.Open(path);
                foreach (var page in pdf.GetPages())
                {
                    sb.AppendLine($"# Page {page.Number}");
                    sb.AppendLine(page.Text);
                    sb.AppendLine();
                }

                return sb.ToString();
            }
            catch (Exception ex)
            {
                return $"# Error reading PDF: {ex.Message}";
            }
        });
    }
}