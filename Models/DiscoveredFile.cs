using System.Text.Json.Serialization;

namespace FileCombiner.Models;

/// <summary>
/// Information about a discovered file
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

/// <summary>
/// Result of processing operation
/// </summary>
public record ProcessResult(
    List<DiscoveredFile> ProcessedFiles,
    List<string> SkippedFiles,
    List<string> SkippedDirectories,
    long TotalSize,
    int TokenCount,
    int FoundDirs
);

/// <summary>
/// Output data structure for JSON export
/// </summary>
public record OutputData(
    [property: JsonPropertyName("summary")] SummaryData Summary,
    [property: JsonPropertyName("files")] List<DiscoveredFile> Files
);

public record SummaryData(
    [property: JsonPropertyName("total_files")] int TotalFiles,
    [property: JsonPropertyName("total_size")] long TotalSize,
    [property: JsonPropertyName("total_tokens")] int totalTokens,
    [property: JsonPropertyName("max_depth")] int MaxDepth
);

/// <summary>
/// Output data for combined files
/// </summary>
public record CombinedFilesData(
    [property: JsonPropertyName("FinalContent")] string FinalContent,
    [property: JsonPropertyName("totalTokens")] int TotalTokens
);