// ReSharper disable NotAccessedPositionalProperty.Global
namespace FileCombiner.Modules.Models;

/// <summary>
///     Information about a discovered file
/// </summary>
public record DiscoveredFile(
    string RelativePath,
    string AbsolutePath,
    long Size,
    int Depth
)
{
    public string Extension => Path.GetExtension(RelativePath).ToLowerInvariant();
    public string Name => Path.GetFileName(RelativePath);
}