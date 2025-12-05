using System.Text;
// ReSharper disable UnusedType.Global

namespace FileCombiner.Modules.Services;

/// <summary>
///     Service for detecting if files contain readable text - Interface Segregation Principle
/// </summary>
public interface ITextDetectionService
{
    Task<(bool IsText, string Encoding)> IsTextFileAsync(string filePath);
}

/// <summary>
///     Implementation of text detection using multiple heuristics
/// </summary>
public class TextDetectionService : ITextDetectionService
{
    private const int SampleSize = 8192;
    private const double PrintableThreshold = 0.7;

    public async Task<(bool IsText, string Encoding)> IsTextFileAsync(string filePath)
    {
        try
        {
            var fileInfo = new FileInfo(filePath);
            if (fileInfo.Length == 0)
                return (true, "empty");

            var sampleSize = (int)Math.Min(SampleSize, fileInfo.Length);
            var buffer = new byte[sampleSize];

            await using var stream = File.OpenRead(filePath);
            var bytesRead = await stream.ReadAsync(buffer.AsMemory(0, sampleSize));

            // Check for null bytes (binary indicator)
            if (buffer.Take(bytesRead).Contains((byte)0))
                return (false, "binary");

            // Try different encodings
            var encodings = new[] { "utf-8", "utf-16", "latin-1", "cp1252" };

            foreach (var encodingName in encodings)
                try
                {
                    var encoding = Encoding.GetEncoding(encodingName);
                    var text = encoding.GetString(buffer, 0, bytesRead);

                    var printableCount = text.Count(c => char.IsControl(c) ? c is '\n' or '\r' or '\t' : !char.IsControl(c));
                    var printableRatio = (double)printableCount / text.Length;

                    if (printableRatio > PrintableThreshold)
                        return (true, encodingName);
                }
                catch (Exception)
                {
                    // ignored
                }

            return (false, "unknown");
        }
        catch (Exception)
        {
            return (false, "error");
        }
    }
}