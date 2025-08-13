namespace FileCombiner.Services;

/// <summary>
/// Service for detecting programming languages for syntax highlighting
/// </summary>
public interface ILanguageDetectionService
{
    string DetectLanguage(string filename);
}

/// <summary>
/// Implementation using file extensions and common filename patterns
/// </summary>
public class LanguageDetectionService : ILanguageDetectionService
{
    private static readonly Dictionary<string, string> ExtensionMap = new()
    {
        { ".py", "python" }, { ".js", "javascript" }, { ".ts", "typescript" }, { ".jsx", "javascript" },
        { ".tsx", "typescript" }, { ".java", "java" }, { ".c", "c" }, { ".cpp", "cpp" }, { ".cc", "cpp" },
        { ".cxx", "cpp" }, { ".cs", "csharp" }, { ".php", "php" }, { ".rb", "ruby" }, { ".go", "go" },
        { ".rs", "rust" }, { ".kt", "kotlin" }, { ".swift", "swift" }, { ".scala", "scala" },
        { ".sh", "bash" }, { ".sql", "sql" }, { ".r", "r" }, { ".m", "matlab" }, { ".pl", "perl" },
        { ".lua", "lua" }, { ".html", "html" }, { ".htm", "html" }, { ".xml", "xml" }, { ".css", "css" },
        { ".scss", "scss" }, { ".json", "json" }, { ".yaml", "yaml" }, { ".yml", "yaml" },
        { ".toml", "toml" }, { ".md", "markdown" }, { ".rst", "rst" }, { ".tex", "latex" },
        { ".dockerfile", "dockerfile" }, { ".makefile", "makefile" }, { ".ini", "ini" }, { ".cfg", "ini" },
        { ".conf", "ini" }
    };

    private static readonly Dictionary<string, string> FilenameMap = new()
    {
        { "dockerfile", "dockerfile" }, { "makefile", "makefile" }, { "rakefile", "ruby" },
        { "gemfile", "ruby" }, { "requirements.txt", "text" }, { ".gitignore", "text" },
        { ".env", "bash" }, { ".bashrc", "bash" }, { ".zshrc", "bash" }
    };

    public string DetectLanguage(string filename)
    {
        var name = Path.GetFileName(filename).ToLowerInvariant();

        // Check exact filename matches first
        if (FilenameMap.TryGetValue(name, out var language))
            return language;

        // Check extension
        var extension = Path.GetExtension(filename).ToLowerInvariant();
        return ExtensionMap.TryGetValue(extension, out language) ? language : "text";
    }
}