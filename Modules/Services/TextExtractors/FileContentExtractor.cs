namespace FileCombiner.Modules.Services.TextExtractors;

/// <summary>
///     The unified high-level extractor that delegates to specialized ones via the factory.
/// </summary>
public class FileContentExtractor : IFileTextExtractor
{
    private readonly ContentExtractorFactory _factory;

    public FileContentExtractor(ContentExtractorFactory factory)
    {
        _factory = factory;
    }

    public bool CanHandle(string extension)
    {
        return true;
        // acts as universal fallback
    }

    public async Task<string> ExtractTextAsync(string filePath)
    {
        var ext = Path.GetExtension(filePath);
        var extractor = _factory.GetExtractor(ext);

        if (extractor != null && extractor != this) return await extractor.ExtractTextAsync(filePath);

        try
        {
            return await File.ReadAllTextAsync(filePath);
        }
        catch (Exception ex)
        {
            return $"# Error reading file: {ex.Message}";
        }
    }
}