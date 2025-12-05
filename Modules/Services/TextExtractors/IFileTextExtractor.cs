namespace FileCombiner.Modules.Services.TextExtractors;

/// <summary>
///     Interface implemented by all format-specific extractors (and the top-level orchestrator).
/// </summary>
public interface IFileTextExtractor
{
    bool CanHandle(string extension);
    Task<string> ExtractTextAsync(string path);
}