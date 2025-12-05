using System.Text;
using FileCombiner.Modules.CLI;
using FileCombiner.Modules.Configuration;
using FileCombiner.Modules.Models;
using FileCombiner.Modules.Services;
using FileCombiner.Modules.Services.TextExtractors;
using FileCombiner.Modules.Services.TextExtractors.FileTypes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using TextCopy;

namespace FileCombiner.Modules;

/// <summary>
///     Runtime utility helpers for DI configuration and environment I/O.
/// </summary>
public static class RunTimeUtils
{
    public static void ConfigureServices(IServiceCollection services, CommandLineOptions options)
    {
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(options.Verbose ? LogLevel.Debug : LogLevel.Information);
        });

        // register all leaf extractors first (these are the specialized extractors)
        var leafExtractors = new List<IFileTextExtractor>
        {
            new DocxTextExtractor(),
            new ExcelTextExtractor(),
            new CsvTextExtractor(),
            new PdfTextExtractor(),
            new HtmlToMarkdownExtractor(),
            new PptxTextExtractor(),
            new YamlTextExtractor(),
            new MarkdownTextExtractor()
        };

        // register factory with the leaf extractors (avoiding circular dependency)
        var factory = new ContentExtractorFactory(leafExtractors);
        services.AddSingleton(factory);

        // register the main FileContentExtractor that uses the factory
        var mainExtractor = new FileContentExtractor(factory);
        services.AddSingleton<IFileTextExtractor>(mainExtractor);

        // other services
        services.AddSingleton<IFileDiscoveryService, FileDiscoveryService>();
        services.AddSingleton<IFileCombinerService, FileCombinerService>();
        services.AddSingleton<ITextDetectionService, TextDetectionService>();
        services.AddSingleton<ILanguageDetectionService, LanguageDetectionService>();
    }

    public static void AddTextParser(IServiceCollection services, AppConfig config)
    {
        services.AddSingleton<ITextMatcherService>(_ => new FileSystemTextGlobber(config.IncludePatterns, config.ExcludePatterns));
    }

    public static async Task CopyToClipboard(CombinedFilesData data)
    {
        try
        {
            await ClipboardService.SetTextAsync(data.FinalContent);
            AnsiConsole.MarkupLine(
                $"📋 Output [yellow]copied[/] to clipboard ([magenta]{data.TotalTokens:N0}[/] tokens)");
        }
        catch (Exception ex)
        {
            var tempFile = Path.Combine(Path.GetTempPath(),
                $"combined-files-{DateTime.Now:yyyyMMdd-HHmmss}.md");
            await File.WriteAllTextAsync(tempFile, data.FinalContent, Encoding.UTF8);

            AnsiConsole.MarkupLine($"[yellow]⚠️ Clipboard failed, saved to:[/] {tempFile}");
            AnsiConsole.MarkupLine($"[dim]Error: {ex.Message}[/]");
        }
    }

    public static string BuildDirectoryTree(IEnumerable<DiscoveredFile> files, AppConfig config)
    {
        var root = new DirectoryNode(".");
        foreach (var file in files)
        {
            var relPath = file.RelativePath.Replace('\\', '/');
            var parts = relPath.Split('/', StringSplitOptions.RemoveEmptyEntries);
            var current = root;

            for (var depth = 0; depth < parts.Length - 1 && depth < config.MaxDepth; depth++)
            {
                var part = parts[depth];
                if (!current.Children.TryGetValue(part, out var child))
                {
                    child = new DirectoryNode(part);
                    current.Children[part] = child;
                }

                current = child;
            }
        }

        var sb = new StringBuilder();
        sb.AppendLine("## Directory Structure\n");
        RenderNode(sb, root, 0, config.MaxDepth);
        sb.AppendLine("\n" + new string('-', 79) + "\n");
        return sb.ToString();
    }

    private static void RenderNode(StringBuilder sb, DirectoryNode node, int indent, int maxDepth)
    {
        if (indent > 0)
            sb.AppendLine($"{new string(' ', (indent - 1) * 2)}- `{node.Name}`");

        if (indent >= maxDepth) return;

        foreach (var child in node.Children.Values.OrderBy(c => c.Name))
            RenderNode(sb, child, indent + 1, maxDepth);
    }

    private sealed class DirectoryNode
    {
        public DirectoryNode(string name)
        {
            Name = name;
        }

        public string Name { get; }
        public Dictionary<string, DirectoryNode> Children { get; } = new(StringComparer.OrdinalIgnoreCase);
    }
}