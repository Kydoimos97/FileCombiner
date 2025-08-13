using CommandLine;
using JetBrains.Annotations;

namespace FileCombiner.CLI;

/// <summary>
/// Command line options using CommandLineParser library (no defaults here).
/// Defaults live in AppConfig.CreateDefault (SSOT).
/// </summary>
[Verb("combine", isDefault: true, HelpText = "Combine files from a directory into a single reference document")]
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class CommandLineOptions
{
    [Value(0, Required = true, HelpText = "Source directory to scan")]
    public string Directory { get; set; } = string.Empty;

    [Option('o', "output", HelpText = "Output file path (default: copy to clipboard)")]
    public string? OutputFile { get; set; }

    [Option('e', "extensions", Separator = ',',
        HelpText = "File extensions to include (comma-separated, use '*' for auto-detect)")]
    public IEnumerable<string>? Extensions { get; set; }

    [Option('r', "max-depth", HelpText = "Maximum directory depth to recurse")]
    public int? MaxDepth { get; set; }

    [Option("include", Separator = ',', HelpText = "Include files matching these patterns")]
    public IEnumerable<string>? IncludePatterns { get; set; }

    [Option("exclude", Separator = ',', HelpText = "Exclude files/dirs matching these patterns")]
    public IEnumerable<string>? ExcludePatterns { get; set; }

    [Option("ignore-dirs", Separator = ',', HelpText = "Additional directories to ignore")]
    public IEnumerable<string>? IgnoreDirs { get; set; }

    [Option('c', "compact", HelpText = "Enable compact mode (remove comments, docstrings)")]
    public bool CompactMode { get; set; }

    [Option("no-tree", HelpText = "Don't include directory tree in output")]
    public bool NoTree { get; set; }

    [Option("max-files", HelpText = "Maximum number of files to process")]
    public int? MaxFiles { get; set; }

    [Option("max-file-size", HelpText = "Maximum file size in bytes (default: 10MB)")]
    public long? MaxFileSize { get; set; }

    [Option("dry-run", HelpText = "Show what would be processed without actually doing it")]
    public bool DryRun { get; set; }

    [Option('v', "verbose", HelpText = "Enable verbose output")]
    public bool Verbose { get; set; }
}
