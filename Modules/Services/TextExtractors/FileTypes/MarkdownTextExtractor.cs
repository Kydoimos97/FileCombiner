namespace FileCombiner.Modules.Services.TextExtractors.FileTypes;

public class MarkdownTextExtractor : IFileTextExtractor
{
    public bool CanHandle(string ext)
    {
        return ext.Equals(".md", StringComparison.OrdinalIgnoreCase);
    }

    public async Task<string> ExtractTextAsync(string path)
    {
        return await File.ReadAllTextAsync(path); // raw passthrough
    }
}