// ReSharper disable UnusedType.Global
using FileCombiner.Modules.Configuration;
using FileCombiner.Modules.Models;
using Microsoft.Extensions.Logging;
using Spectre.Console;

namespace FileCombiner.Modules.Services;

/// <summary>
///     Service for discovering files in directory structure with verbose logging and Spectre markup.
/// </summary>
public interface IFileDiscoveryService
{
    Task<ProcessResult> DiscoverFilesAsync(AppConfig config);
}

public class FileDiscoveryService : IFileDiscoveryService
{
    private readonly ITextDetectionService _textDetectionService;
    private readonly ITextMatcherService _matcher;
    private readonly ILogger<FileDiscoveryService> _logger;

    public FileDiscoveryService(ITextDetectionService textDetectionService, ITextMatcherService matcher, ILogger<FileDiscoveryService> logger)
    {
        _textDetectionService = textDetectionService;
        _matcher = matcher;
        _logger = logger;
    }

    public async Task<ProcessResult> DiscoverFilesAsync(AppConfig config)
    {
        _logger.LogDebug("DiscoverFilesAsync: Method entry point reached");
        
        var foundFiles = new List<DiscoveredFile>();
        var skippedFiles = new List<string>();
        var skippedDirs = new List<string>();

        _logger.LogDebug("DiscoverFilesAsync: Initialized collections");
        
        AnsiConsole.MarkupLine($"🔍 Scanning [cyan]{Esc(config.Directory)}[/] (max depth: {config.MaxDepth})...");
        _logger.LogInformation("Starting directory scan in {Directory} with depth={Depth}", config.Directory, config.MaxDepth);
        _logger.LogDebug("DiscoverFilesAsync: About to call ScanDirectory");

        var startTime = DateTime.UtcNow;

        var foundDirs = await ScanDirectory(new DirectoryInfo(config.Directory), config, foundFiles, skippedFiles, skippedDirs, 0)
            .ConfigureAwait(false);

        _logger.LogDebug("DiscoverFilesAsync: ScanDirectory completed, processing results");
        
        var totalSize = foundFiles.Sum(f => f.Size);
        var folderCount = foundFiles.Select(f => Path.GetDirectoryName(f.RelativePath)).Distinct().Count();

        var elapsed = DateTime.UtcNow - startTime;
        _logger.LogInformation("Discovery complete: {Files} files, {Dirs} dirs, {Elapsed} ms",
            foundFiles.Count, folderCount, elapsed.TotalMilliseconds);
        
        _logger.LogDebug("DiscoverFilesAsync: Calculated statistics - totalSize={Size}, folderCount={Count}", totalSize, folderCount);

        AnsiConsole.MarkupLine($"✅ Found [green]{folderCount}[/] folders and [cyan]{foundFiles.Count}[/] files in [dim]{elapsed.TotalMilliseconds:N0} ms[/].");

        // --- print summary results ---
        if (foundFiles.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]No files found to process.[/]");
            return new ProcessResult(foundFiles, skippedFiles, skippedDirs, totalSize, 0, foundDirs);
        }

        if (config.Verbose)
        {
            var list = foundFiles.Count <= 20
                ? foundFiles
                : foundFiles.Take(10).ToList();

            foreach (var (file, i) in list.Select((f, i) => (f, i)))
            {
                var sizeInfo = file.Size > 0 ? $" [dim]({file.Size:N0} bytes)[/]" : "";
                AnsiConsole.MarkupLine($"  [green]{i + 1}:[/] [grey]{Esc(file.RelativePath)}[/]{sizeInfo}");
            }

            if (foundFiles.Count > 20)
                AnsiConsole.MarkupLine($"  [dim]... and {foundFiles.Count - 10} more files[/]");
        }

        if (skippedDirs.Count > 0)
        {
            AnsiConsole.MarkupLine($"📁 Skipped [red]{skippedDirs.Count}[/] directories");
            _logger.LogDebug("Skipped {Count} directories", skippedDirs.Count);
            if (config.Verbose)
                foreach (var dir in skippedDirs.Take(5))
                    _logger.LogTrace("  SkippedDir={Dir}", dir);
        }

        if (skippedFiles.Count > 0)
        {
            AnsiConsole.MarkupLine($"📄 Skipped [red]{skippedFiles.Count}[/] files");
            _logger.LogDebug("Skipped {Count} files", skippedFiles.Count);
            if (config.Verbose)
                foreach (var file in skippedFiles.Take(5))
                    _logger.LogTrace("  SkippedFile={File}", file);
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
        if (currentDepth > config.MaxDepth)
        {
            _logger.LogDebug("Skipping {Dir} (depth {Depth} > {Max})", directory.FullName, currentDepth, config.MaxDepth);
            return 0;
        }

        var opts = new EnumerationOptions { RecurseSubdirectories = false, IgnoreInaccessible = true };
        var subdirCount = 0;

        try
        {
            // files
            foreach (var file in directory.EnumerateFiles("*", opts))
            {
                if (foundFiles.Count >= config.MaxTotalFiles)
                {
                    _logger.LogWarning("Reached file limit ({Limit}), stopping.", config.MaxTotalFiles);
                    break;
                }

                var relativePath = Path.GetRelativePath(config.Directory, file.FullName);
                var fi = new DiscoveredFile(relativePath, file.FullName, file.Length, currentDepth);

                var (skip, reason) = await ShouldSkipFileAsync(fi, config).ConfigureAwait(false);
                if (skip)
                {
                    skippedFiles.Add($"{relativePath} {reason}");
                    if (config.Verbose)
                        _logger.LogTrace("Skipped file {File}: {Reason}", relativePath, reason);
                    continue;
                }

                foundFiles.Add(fi);
                if (config.Verbose && foundFiles.Count % 25 == 0)
                    _logger.LogInformation("Discovered {Count} files so far...", foundFiles.Count);
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
                    if (config.Verbose)
                        _logger.LogTrace("Skipped directory {Dir}: {Reason}", rel, reason);
                    continue;
                }

                subdirCount += 1;
                subdirCount += await ScanDirectory(subDir, config, foundFiles, skippedFiles, skippedDirs, currentDepth + 1)
                    .ConfigureAwait(false);
            }
        }
        catch (UnauthorizedAccessException)
        {
            var rel = Path.GetRelativePath(config.Directory, directory.FullName);
            skippedDirs.Add($"{Esc(rel)} | [red]access denied[/]");
            _logger.LogWarning("Access denied to {Dir}", directory.FullName);
        }
        catch (Exception ex)
        {
            var rel = Path.GetRelativePath(config.Directory, directory.FullName);
            skippedDirs.Add($"{Esc(rel)} | [red]error:[/] {Markup.Escape(ex.Message)}");
            _logger.LogError(ex, "Error scanning directory {Dir}", directory.FullName);
        }

        return subdirCount;
    }

    private (bool ShouldSkip, string Reason) ShouldSkipDirectory(DirectoryInfo dir, AppConfig config, int depth)
    {
        if (depth > config.MaxDepth)
            return (true, $"| exceeds max depth ([yellow]{config.MaxDepth}[/])");

        if (config.IgnoreDirs.Contains(dir.Name))
            return (true, "| in ignore list");

        var rel = Path.GetRelativePath(config.Directory, dir.FullName);
        var match = _matcher.IsMatch(rel, true);
        if (!match)
            _logger.LogTrace("Directory {Dir} excluded by matcher", rel);

        return !match ? (true, "| excluded by glob pattern") : (false, string.Empty);
    }

    private async Task<(bool ShouldSkip, string Reason)> ShouldSkipFileAsync(DiscoveredFile fileInfo, AppConfig config)
    {
        if (fileInfo.Size > config.MaxFileSize)
            return (true, $"| too large ([yellow]{fileInfo.Size:N0}[/] bytes)");

        if (config.IgnoreFiles.Contains(fileInfo.Name))
            return (true, "| in ignore list");

        if (!string.IsNullOrEmpty(config.OutputFile))
        {
            var outputPath = Path.GetFullPath(config.OutputFile);
            if (fileInfo.AbsolutePath.Equals(outputPath, StringComparison.OrdinalIgnoreCase))
                return (true, "| is output file");
        }

        if (config.AutoDetectText)
        {
            var (isText, encoding) = await _textDetectionService.IsTextFileAsync(fileInfo.AbsolutePath).ConfigureAwait(false);
            if (!isText)
            {
                _logger.LogTrace("File {File} skipped (binary: {Encoding})", fileInfo.RelativePath, encoding);
                return (true, $" | not a text file ([yellow]{encoding}[/])");
            }
        }
        else if (!config.Extensions.Contains(fileInfo.Extension))
        {
            _logger.LogTrace("File {File} skipped (extension {Ext} not included)", fileInfo.RelativePath, fileInfo.Extension);
            return (true, $"| extension not included ([yellow]{fileInfo.Extension}[/])");
        }

        var match = _matcher.IsMatch(fileInfo.RelativePath);
        if (!match)
            _logger.LogTrace("File {File} excluded by matcher", fileInfo.RelativePath);

        return !match ? (true, "| excluded by glob patterns") : (false, string.Empty);
    }

    private static string Esc(string s) => Markup.Escape(s);
}
