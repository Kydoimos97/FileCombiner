using System.Text;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace FileCombiner.Modules.Services.TextExtractors.FileTypes;

public class DocxTextExtractor : IFileTextExtractor
{
    public bool CanHandle(string extension)
    {
        return string.Equals(extension, ".docx", StringComparison.OrdinalIgnoreCase);
    }

    public async Task<string> ExtractTextAsync(string path)
    {
        return await Task.Run(() =>
        {
            var sb = new StringBuilder();

            using var doc = WordprocessingDocument.Open(path, false);
            var mainPart = doc.MainDocumentPart;
            if (mainPart?.Document.Body is null)
                return string.Empty;

            foreach (var paragraph in mainPart.Document.Body.Descendants<Paragraph>())
                sb.AppendLine(paragraph.InnerText);

            return sb.ToString();
        });
    }
}