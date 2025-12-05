// ReSharper disable UnusedType.Global
using System.Text;
using FileCombiner.Modules.Configuration;
using FileCombiner.Modules.Models;
using FileCombiner.Modules.Services.TextExtractors;
using Microsoft.Extensions.Logging;

namespace FileCombiner.Modules.Services;

/// <summary>
///     Main service orchestrating the file combination process - Facade Pattern.
///     Responsible for orchestrating discovery, extraction, and final document assembly.
/// </summary>
public interface IFileCombinerService
{
    Task<CombinedFilesData> CombineFilesAsync(AppConfig config, ProcessResult result);
}

/// <summary>
///     Implementation of the main file combination logic.
///     Delegates extraction to IFileTextExtractor implementations and tree building to RunTimeUtils.
/// </summary>
public class FileCombinerService : IFileCombinerService
{
    private static readonly char[] TokenSeps = [' ', '\t', '\n', '\r', '.', ',', ';', ':', '!', '?'];
    private readonly ILanguageDetectionService _languageService;
    private readonly ILogger<FileCombinerService> _logger;
    private readonly IFileTextExtractor _textExtractor;

    public FileCombinerService(ILanguageDetectionService languageService,
        ILogger<FileCombinerService> logger,
        IFileTextExtractor textExtractor)
    {
        _languageService = languageService;
        _logger = logger;
        _textExtractor = textExtractor;
    }

    public async Task<CombinedFilesData> CombineFilesAsync(AppConfig config, ProcessResult result)
    {
        if (result.ProcessedFiles.Count == 0)
            return new CombinedFilesData(string.Empty, 0);

        var output = new StringBuilder();

        // Document header
        output.AppendLine("## Combined Files Reference");
        output.AppendLine();
        output.AppendLine("This is a combined file representative of a folder structure, only used as reference.");
        output.AppendLine();

        // Directory tree section
        if (config.IncludeTree)
            output.Append(RunTimeUtils.BuildDirectoryTree(result.ProcessedFiles, config));

        // Per-file extraction and inclusion
        foreach (var fileInfo in result.ProcessedFiles)
            await ProcessFile(output, fileInfo, config);

        var finalContent = output.ToString();
        var tokenCount = EstimateTokens(finalContent);

        return new CombinedFilesData(finalContent, tokenCount);
    }

    private async Task ProcessFile(StringBuilder output, DiscoveredFile discoveredFile, AppConfig config)
    {
        try
        {
            // File header
            output.AppendLine($"### `{discoveredFile.RelativePath}`");

            // Syntax highlighting language guess
            var language = _languageService.DetectLanguage(discoveredFile.RelativePath);
            output.AppendLine($"```{language}");

            // Extract text content using unified extractor
            var content = await _textExtractor.ExtractTextAsync(discoveredFile.AbsolutePath);

            if (string.IsNullOrWhiteSpace(content))
            {
                output.AppendLine("# Empty file");
            }
            else
            {
                var processed = ProcessContent(content, config);
                output.AppendLine(processed);
            }

            output.AppendLine("```");
            output.AppendLine();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing file {FilePath}", discoveredFile.AbsolutePath);
            output.AppendLine($"# Error reading file: {ex.Message}");
            output.AppendLine("```");
            output.AppendLine();
        }
    }

    private static string ProcessContent(string content, AppConfig config)
    {
        if (!config.CompactMode)
            return content;

        var lines = content.Split('\n');
        var result = new List<string>();
        var inCopyright = false;

        foreach (var line in lines)
        {
            var trimmed = line.Trim();

            if (trimmed.Contains("copyright", StringComparison.OrdinalIgnoreCase) && trimmed.StartsWith('#'))
            {
                inCopyright = true;
                continue;
            }

            if (inCopyright && (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith('#')))
                continue;

            inCopyright = false;

            if (string.IsNullOrEmpty(trimmed) && result.LastOrDefault() == string.Empty)
                continue;

            result.Add(line.TrimEnd());
        }

        return string.Join('\n', result);
    }

    private static int EstimateTokens(string text)
    {
        return text.Split(TokenSeps, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Length;
    }
}