// ReSharper disable NotAccessedPositionalProperty.Global
using System.Text.Json.Serialization;

namespace FileCombiner.Modules.Models;

/// <summary>
///     Output data for combined files
/// </summary>
public record CombinedFilesData(
    [property: JsonPropertyName("FinalContent")]
    string FinalContent,
    [property: JsonPropertyName("totalTokens")]
    int TotalTokens
);