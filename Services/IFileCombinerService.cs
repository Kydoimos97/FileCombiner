using System.Text;
using FileCombiner.Configuration;
using FileCombiner.Models;
using Microsoft.Extensions.Logging;


namespace FileCombiner.Services;
/// <summary>
/// Main service orchestrating the file combination process - Facade Pattern
/// </summary>
public interface IFileCombinerService
{
    Task<CombinedFilesData> CombineFilesAsync(AppConfig config, ProcessResult result);
}

/// <summary>
/// Implementation of the main file combination logic
/// </summary>
public class FileCombinerService : IFileCombinerService
{
    private readonly IFileDiscoveryService _discoveryService;
    private readonly ILanguageDetectionService _languageService;
    private readonly ILogger<FileCombinerService> _logger;
    private static readonly char[] TokenSeps = [' ', '\t', '\n', '\r', '.', ',', ';', ':', '!', '?'];

    // TODO: Extract Presentation Logic to Dedicated Class
    public FileCombinerService(
        IFileDiscoveryService discoveryService,
        ILanguageDetectionService languageService,
        ILogger<FileCombinerService> logger)
    {
        _discoveryService = discoveryService;
        _languageService = languageService;
        _logger = logger;
    }

    public async Task<CombinedFilesData> CombineFilesAsync(AppConfig config, ProcessResult result)
    {
        // Discovery phase

        if (!result.ProcessedFiles.Any())
        {

            return new CombinedFilesData(string.Empty, 0);
        }

        // Build content
        var output = new StringBuilder();

        // Header
        output.AppendLine("## Combined Files Reference");
        output.AppendLine();
        output.AppendLine("This is a combined file representative of a folder structure, only used as reference.");
        output.AppendLine();

        // Directory tree
        if (config.IncludeTree)
        {
            BuildDirectoryTree(output, result.ProcessedFiles);
        }

        // Process files
        foreach (var fileInfo in result.ProcessedFiles)
        {
            // Use plain text format instead of markup

            await ProcessFile(output, fileInfo, config);
        }

        var finalContent = output.ToString();

        // Simple token estimation
        var tokenCount = EstimateTokens(finalContent);

        return new CombinedFilesData(finalContent, tokenCount);
    }

    private static void BuildDirectoryTree(StringBuilder output, List<DiscoveredFile> files)
    {
        output.AppendLine("## Directory Structure\n");

        var dirs = files
            .Select(f => Path.GetDirectoryName(f.RelativePath) ?? string.Empty)
            .SelectMany(dir => ExpandParents(dir))
            .Where(d => d.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(d => d)
            .ToList();

        foreach (var d in dirs)
            output.AppendLine($"- `{d.Replace('\\','/')}`");

        output.AppendLine("\n" + new string('-', 79) + "\n");
    }

    private static IEnumerable<string> ExpandParents(string dir)
    {
        var parts = dir.Replace('\\','/').Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) yield break;

        var current = parts[0];
        yield return current;

        for (int i = 1; i < parts.Length; i++)
        {
            current += "/" + parts[i];
            yield return current;
        }
    }



    private async Task ProcessFile(StringBuilder output, DiscoveredFile discoveredFile, AppConfig config)
    {
        try
        {
            // File header
            output.AppendLine($"### `{discoveredFile.RelativePath}`");

            // Language detection
            var language = _languageService.DetectLanguage(discoveredFile.RelativePath);
            output.AppendLine($"```{language}");

            // Read and process content
            var content = await File.ReadAllTextAsync(discoveredFile.AbsolutePath);

            if (string.IsNullOrWhiteSpace(content))
            {
                output.AppendLine("# Empty file");
            }
            else
            {
                var processedContent = ProcessContent(content, config);
                output.AppendLine(processedContent);
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

        // Simple compact mode - remove copyright headers and excessive whitespace
        var lines = content.Split('\n');
        var result = new List<string>();
        var inCopyright = false;

        foreach (var line in lines)
        {
            var trimmed = line.Trim();

            // Skip copyright blocks
            if (trimmed.Contains("copyright", StringComparison.OrdinalIgnoreCase) && trimmed.StartsWith('#'))
            {
                inCopyright = true;
                continue;
            }

            if (inCopyright && (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith('#')))
            {
                continue;
            }

            inCopyright = false;

            // Skip excessive blank lines
            if (string.IsNullOrEmpty(trimmed) && result.LastOrDefault() == string.Empty)
                continue;

            result.Add(line.TrimEnd());
        }

        return string.Join('\n', result);
    }

    private static int EstimateTokens(string text)
    {
        // Simple estimation: split by whitespace and punctuation
        return text.Split(TokenSeps, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Length;
    }
}