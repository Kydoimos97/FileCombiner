using CommandLine;
using JetBrains.Annotations;

namespace FileCombiner.Modules.CLI;

/// <summary>
///     Command line options using CommandLineParser library (no defaults here).
///     Defaults live in AppConfig.CreateDefault (SSOT).
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class CommandLineOptions
{
    [Value(0, Required = false, HelpText = "Source directory to scan (default: current directory)")]
    public string Directory { get; set; } = ".";

    [Option('o', "output", HelpText = "Output file path (default: copy to clipboard)")]
    public string? OutputFile { get; set; }

    [Option('e', "extensions", Separator = ',',
        HelpText = "File extensions to include (comma-separated, use '*' for auto-detect)")]
    public IEnumerable<string>? Extensions { get; set; }

    [Option('r', "max-depth", HelpText = "Maximum directory depth to recurse (default: 5)")]
    public int? MaxDepth { get; set; }

    [Option("exclude", Separator = ',', HelpText = "Exclude files/dirs matching glob patterns")]
    public IEnumerable<string>? ExcludePatterns { get; set; }

    [Option("dry-run", HelpText = "Preview files without processing")]
    public bool DryRun { get; set; }

    [Option('v', "verbose", HelpText = "Enable detailed logging output")]
    public bool Verbose { get; set; }

    [Option('i', "interactive", HelpText = "Enter interactive mode with prompts")]
    public bool Interactive { get; set; }

    // Kept for backward compatibility but not exposed in help
    [Option("include", Separator = ',', Hidden = true)]
    public IEnumerable<string>? IncludePatterns { get; set; }

    [Option("ignore-dirs", Separator = ',', Hidden = true)]
    public IEnumerable<string>? IgnoreDirs { get; set; }

    [Option('c', "compact", Hidden = true)]
    public bool CompactMode { get; set; }

    [Option("no-tree", Hidden = true)]
    public bool NoTree { get; set; }

    [Option("max-files", Hidden = true)]
    public int? MaxFiles { get; set; }

    [Option("max-file-size", Hidden = true)]
    public long? MaxFileSize { get; set; }
}