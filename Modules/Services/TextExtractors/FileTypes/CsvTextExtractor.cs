namespace FileCombiner.Modules.Services.TextExtractors.FileTypes;

public class CsvTextExtractor : IFileTextExtractor
{
    public bool CanHandle(string extension)
    {
        return extension.Equals(".csv", StringComparison.OrdinalIgnoreCase);
    }

    public async Task<string> ExtractTextAsync(string path)
    {
        var lines = await File.ReadAllLinesAsync(path);
        return string.Join(Environment.NewLine, lines);
    }
}