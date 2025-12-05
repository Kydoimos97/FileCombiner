// ReSharper disable NotAccessedPositionalProperty.Global
namespace FileCombiner.Modules.Models;

/// <summary>
///     Result of processing operation
/// </summary>
public record ProcessResult(
    List<DiscoveredFile> ProcessedFiles,
    List<string> SkippedFiles,
    List<string> SkippedDirectories,
    long TotalSize,
    int TokenCount,
    int FoundDirs
);