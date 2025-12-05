using FileCombiner.Modules.CLI;
using JetBrains.Annotations;

namespace FileCombiner.Modules.Configuration;

/// <summary>
///     Application configuration - single source of truth for defaults.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class AppConfig : ConfigObject
{
    public string Directory { get; private init; } = string.Empty;
    public string? OutputFile { get; set; }
    public List<string> Extensions { get; set; } = ["*"];
    public int MaxDepth { get; set; } = 5;
    public bool CompactMode { get; private set; }
    public bool IncludeTree { get; private set; } = true;
    public List<string> ExcludePatterns { get; set; } = [];
    public List<string> IncludePatterns { get; set; } = [];
    public HashSet<string> IgnoreDirs { get; private set; } = [];
    public HashSet<string> IgnoreFiles { get; private init; } = [];
    public long MaxFileSize { get; set; } = 10 * 1024 * 1024; // 10MB
    public int MaxTotalFiles { get; set; } = 1000;
    public bool DryRun { get; set; }
    public bool Verbose { get; set; }

    // exactly one element equal to "*"
    public bool AutoDetectText => Extensions is ["*"];

    public static AppConfig CreateDefault(string directory)
    {
        return new AppConfig
        {
            Directory = directory,
            IgnoreDirs =
            [
                "__pycache__", ".git", ".svn", ".hg", ".bzr", "_darcs",
                "node_modules", ".venv", "venv", "env", ".env",
                "build", "dist", ".tox", ".pytest_cache", ".mypy_cache",
                "target", "bin", "obj", ".vs", ".vscode", ".idea",
                "coverage", ".coverage", "htmlcov", ".nyc_output"
            ],
            IgnoreFiles = [".DS_Store", "Thumbs.db", "desktop.ini", ".gitkeep"]
        };
    }

    public static AppConfig FromCommandLine(CommandLineOptions opts)
    {
        var c = CreateDefault(opts.Directory);

        c.OutputFile = UseIfProvided(c.OutputFile, opts.OutputFile);
        c.Extensions = UseIfProvided(c.Extensions, opts.Extensions?.ToList());
        c.MaxDepth = UseIfProvided(c.MaxDepth, opts.MaxDepth);
        c.IncludePatterns = UseIfProvided(c.IncludePatterns, opts.IncludePatterns?.ToList());
        c.ExcludePatterns = UseIfProvided(c.ExcludePatterns, opts.ExcludePatterns?.ToList());
        c.IgnoreDirs = UnionIfProvided(c.IgnoreDirs, opts.IgnoreDirs);
        c.MaxTotalFiles = UseIfProvided(c.MaxTotalFiles, opts.MaxFiles);
        c.MaxFileSize = UseIfProvided(c.MaxFileSize, opts.MaxFileSize);

        c.CompactMode = opts.CompactMode;
        c.IncludeTree = !opts.NoTree;
        c.DryRun = opts.DryRun;
        c.Verbose = opts.Verbose;

        c.Extensions = NormalizeExt(c.Extensions);
        return c;
    }

    private static List<string> NormalizeExt(List<string> exts)
    {
        // If wildcard is present *alone*, keep it as-is.
        if (exts is ["*"]) return exts;

        var outList = new List<string>(exts.Count);
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
        foreach (var raw in exts)
        {
            var e = raw.Trim();
            if (string.IsNullOrEmpty(e)) continue;
            if (e == "*")
            {
                outList.Clear();
                outList.Add("*");
                return outList;
            } // treat lone * as special

            if (!e.StartsWith('.')) e = "." + e;
            if (seen.Add(e)) outList.Add(e);
        }

        // if ended up empty, fall back to "*"
        return outList.Count > 0 ? outList : ["*"];
    }
}