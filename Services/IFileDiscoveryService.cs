using FileCombiner.Configuration;
using FileCombiner.Models;
using Microsoft.Extensions.Logging;
using Spectre.Console;

namespace FileCombiner.Services;
/// <summary>
/// Service for discovering files in directory structure
/// </summary>
public interface IFileDiscoveryService
{
    Task<ProcessResult> DiscoverFilesAsync(AppConfig config);
}

/// <summary>
/// Simple, straightforward file discovery implementation with proper Spectre.Console colors
/// </summary>
public class FileDiscoveryService(ITextDetectionService textDetectionService, ITextMatcherService matcher) : IFileDiscoveryService
{
    static string Esc(string s) => Markup.Escape(s);

    public async Task<ProcessResult> DiscoverFilesAsync(AppConfig config)
    {
        var foundFiles = new List<DiscoveredFile>();
        var skippedFiles = new List<string>();
        var skippedDirs = new List<string>();

        // Use Spectre.Console directly instead of progress reporting
        AnsiConsole.MarkupLine($"🔍 Scanning [cyan]{Esc(config.Directory)}[/] (max depth: {config.MaxDepth})...");

        // Simple recursive file discovery
        var foundDirs = await ScanDirectory(new DirectoryInfo(config.Directory), config, foundFiles, skippedFiles, skippedDirs, 0);


        // Calculate totals
        var totalSize = foundFiles.Sum(f => f.Size);
        var folderCount = foundFiles.Select(f => Path.GetDirectoryName(f.RelativePath)).Distinct().Count();

        // Report results like Python did with proper Spectre colors
        AnsiConsole.MarkupLine($"Found [green]{folderCount}[/] folders and [cyan]{foundFiles.Count}[/] files.");

        if (foundFiles.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]No files found to process.[/]");
            return new ProcessResult(foundFiles, skippedFiles, skippedDirs, totalSize, 0, foundDirs);
        }

        // Show files like Python did with proper numbering and colors
        if (foundFiles.Count <= 20)
        {
            // Show all files if not too many
            for (int i = 0; i < foundFiles.Count; i++)
            {
                var file = foundFiles[i];
                var sizeInfo = file.Size > 0 ? $" [dim]([cyan]{file.Size:N0}[/] bytes)[/]" : "";
                // Use proper Spectre markup - no square brackets in content
                AnsiConsole.MarkupLine($"    [green]{i + 1}:[/][dim]{Esc(file.RelativePath)}[/]{sizeInfo}");
            }
        }
        else
        {
            // Show first 10 files
            for (int i = 0; i < 10; i++)
            {
                var file = foundFiles[i];
                var sizeInfo = file.Size > 0 ? $" ({file.Size:N0} bytes)" : "";
                AnsiConsole.MarkupLine($"    [green]{i + 1}:[/][dim]{Esc(file.RelativePath)}[/]{sizeInfo}");
            }
            AnsiConsole.MarkupLine($"    [dim]... and {foundFiles.Count - 10} more files[/]");
        }

        // Report skipped items with proper colors
        if (skippedDirs.Count > 0)
        {
            AnsiConsole.MarkupLine($"📁 Skipped [red]{skippedDirs.Count}[/] directories");
            if (skippedDirs.Count <= 5)
            {
                for (int i = 0; i < skippedDirs.Count; i++)
                {
                    AnsiConsole.MarkupLine($"   [yellow]{i + 1}:[/][dim]{skippedDirs[i]}[/]");
                }
            }
            else
            {
                for (int i = 0; i < 5; i++)
                {
                    AnsiConsole.MarkupLine($"   [yellow]{i + 1}:[/][dim]{skippedDirs[i]}[/]");
                }
                AnsiConsole.MarkupLine($"   [dim]... and {skippedDirs.Count - 3} more[/]");
            }
        }

        if (skippedFiles.Count > 0)
        {
            AnsiConsole.MarkupLine($"📄 Skipped [red]{skippedFiles.Count}[/] files");
            if (skippedFiles.Count <= 5)
            {
                for (int i = 0; i < skippedFiles.Count; i++)
                {
                    AnsiConsole.MarkupLine($"   - [dim]{skippedFiles[i]}[/]");
                }
            }
            else
            {
                for (int i = 0; i < 3; i++)
                {
                    AnsiConsole.MarkupLine($"   - [yellow]{i + 1}:[/]{skippedFiles[i]}");
                }
                AnsiConsole.MarkupLine($"   [dim]... and {skippedFiles.Count - 3} more[/]");
            }
        }

        return new ProcessResult(foundFiles, skippedFiles, skippedDirs, totalSize, 0, foundDirs);
    }

    private async Task<int> ScanDirectory(
        DirectoryInfo directory,
        AppConfig config,
        List<DiscoveredFile> foundFiles,
        List<string> skippedFiles,
        List<string> skippedDirs,
        int currentDepth)
    {
        if (currentDepth > config.MaxDepth) return 0;
        if (foundFiles.Count >= config.MaxTotalFiles) return 0;

        var opts = new EnumerationOptions {
            RecurseSubdirectories = false,
            IgnoreInaccessible = true,
        };

        var subdirCount = 0;

        try
        {
            // files
            foreach (var file in directory.EnumerateFiles("*", opts))
            {
                if (foundFiles.Count >= config.MaxTotalFiles) break;

                var relativePath = Path.GetRelativePath(config.Directory, file.FullName);

                var fi = new DiscoveredFile(relativePath, file.FullName, file.Length, currentDepth);

                var (skip, reason) = await ShouldSkipFileAsync(fi, config);
                if (skip) { skippedFiles.Add($"{relativePath} {reason}"); continue; }

                foundFiles.Add(fi);
            }

            // directories
            foreach (var subDir in directory.EnumerateDirectories("*", opts))
            {
                if (foundFiles.Count >= config.MaxTotalFiles) break;

                var (skip, reason) = ShouldSkipDirectory(subDir, config, currentDepth + 1);
                if (skip)
                {
                    var rel = Path.GetRelativePath(config.Directory, subDir.FullName);
                    skippedDirs.Add($"{Esc(rel)} {reason}");
                    continue;
                }

                subdirCount += 1;
                subdirCount += await ScanDirectory(subDir, config, foundFiles, skippedFiles, skippedDirs, currentDepth + 1);
            }
        }
        catch (UnauthorizedAccessException)
        {
            var rel = Path.GetRelativePath(config.Directory, directory.FullName);
            skippedDirs.Add($"{Esc(rel)} | [red]access denied[/]");
        }
        catch (Exception ex)
        {
            var rel = Path.GetRelativePath(config.Directory, directory.FullName);
            skippedDirs.Add($"{Esc(rel)} | [red]error:[/] {Markup.Escape(ex.Message)}");
        }

        return subdirCount;
    }


    private (bool ShouldSkip, string Reason) ShouldSkipDirectory(DirectoryInfo dir, AppConfig config, int depth)
    {
        if (depth > config.MaxDepth) return (true, $"| exceeds max depth ([yellow]{config.MaxDepth}[/])");

        if (config.IgnoreDirs.Contains(dir.Name)) return (true, "| in ignore list");

        var rel = Path.GetRelativePath(config.Directory, dir.FullName);

        if (!matcher.IsMatch(rel, isDirectory: true))
        {
            return (true, "| excluded by glob pattern");
        }

        return (false, string.Empty);
    }


    private async Task<(bool ShouldSkip, string Reason)> ShouldSkipFileAsync(DiscoveredFile fileInfo, AppConfig config)
    {
        if (fileInfo.Size > config.MaxFileSize)
            return (true, $"| too large ([yellow]{fileInfo.Size:N0}[/] bytes)");

        if (config.IgnoreFiles.Contains(fileInfo.Name))
            return (true, $"| in ignore list");

        // Check if it's the output file
        if (!string.IsNullOrEmpty(config.OutputFile))
        {
            var outputPath = Path.GetFullPath(config.OutputFile);
            if (fileInfo.AbsolutePath.Equals(outputPath, StringComparison.OrdinalIgnoreCase))
                return (true, "| is output file");
        }

        // Extension/text detection logic
        if (config.AutoDetectText)
        {
            var (isText, encoding) = await textDetectionService.IsTextFileAsync(fileInfo.AbsolutePath);
            if (!isText)
                return (true, $" | not a text file ([yellow]{encoding}[/])");
        }
        else
        {
            if (!config.Extensions.Contains(fileInfo.Extension))
                return (true, $"| extension not included ([yellow]{fileInfo.Extension}[/])");
        }

        if (!matcher.IsMatch(fileInfo.RelativePath))
            return (true, "| excluded by glob patterns");

        return (false, string.Empty);
    }
}