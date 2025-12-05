using System.Text;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Packaging;

namespace FileCombiner.Modules.Services.TextExtractors.FileTypes;

public class PptxTextExtractor : IFileTextExtractor
{
    public bool CanHandle(string ext)
    {
        return ext.Equals(".pptx", StringComparison.OrdinalIgnoreCase);
    }

    public async Task<string> ExtractTextAsync(string path)
    {
        return await Task.Run(() =>
        {
            var sb = new StringBuilder();
            using var pres = PresentationDocument.Open(path, false);
            var slides = pres.PresentationPart?.SlideParts;
            if (slides == null) return sb.ToString();
            foreach (var slide in slides)
            {
                var text = slide.Slide.Descendants<Text>()
                    .Select(t => t.Text)
                    .Where(s => !string.IsNullOrWhiteSpace(s));
                sb.AppendLine(string.Join(Environment.NewLine, text));
                sb.AppendLine();
            }

            return sb.ToString();
        });
    }
}