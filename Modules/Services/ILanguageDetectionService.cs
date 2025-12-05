namespace FileCombiner.Modules.Services;

/// <summary>
///     Service for detecting programming or document languages for syntax highlighting.
/// </summary>
public interface ILanguageDetectionService
{
    string DetectLanguage(string filename);
}

/// <summary>
///     Implementation using file extensions and known filename patterns.
/// </summary>
// ReSharper disable once UnusedType.Global
public class LanguageDetectionService : ILanguageDetectionService
{
    private static readonly Dictionary<string, string> ExtensionMap = new(StringComparer.OrdinalIgnoreCase)
    {
        // Code and scripting languages
        { ".py", "python" }, { ".js", "javascript" }, { ".ts", "typescript" },
        { ".jsx", "javascript" }, { ".tsx", "typescript" }, { ".java", "java" },
        { ".c", "c" }, { ".cpp", "cpp" }, { ".cc", "cpp" }, { ".cxx", "cpp" },
        { ".cs", "csharp" }, { ".php", "php" }, { ".rb", "ruby" }, { ".go", "go" },
        { ".rs", "rust" }, { ".kt", "kotlin" }, { ".swift", "swift" }, { ".scala", "scala" },
        { ".sh", "bash" }, { ".sql", "sql" }, { ".r", "r" }, { ".m", "matlab" },
        { ".pl", "perl" }, { ".lua", "lua" }, { ".html", "html" }, { ".htm", "html" },
        { ".xml", "xml" }, { ".css", "css" }, { ".scss", "scss" },
        { ".json", "json" }, { ".yaml", "yaml" }, { ".yml", "yaml" },
        { ".toml", "toml" }, { ".md", "markdown" }, { ".rst", "rst" },
        { ".tex", "latex" }, { ".dockerfile", "dockerfile" },
        { ".makefile", "makefile" }, { ".ini", "ini" }, { ".cfg", "ini" },
        { ".conf", "ini" },

        // Document and structured text formats
        { ".docx", "text" }, { ".pptx", "text" }, { ".xlsx", "text" },
        { ".pdf", "text" }, { ".csv", "text" }, { ".rtf", "text" },
        { ".log", "text" }
    };

    private static readonly Dictionary<string, string> FilenameMap = new(StringComparer.OrdinalIgnoreCase)
    {
        { "dockerfile", "dockerfile" }, { "makefile", "makefile" }, { "rakefile", "ruby" },
        { "gemfile", "ruby" }, { "requirements.txt", "text" },
        { ".gitignore", "text" }, { ".env", "bash" },
        { ".bashrc", "bash" }, { ".zshrc", "bash" },
        { "package.json", "json" }, { "tsconfig.json", "json" },
        { "README", "markdown" }, { "LICENSE", "text" }
    };

    public string DetectLanguage(string filename)
    {
        var name = Path.GetFileName(filename);
        if (FilenameMap.TryGetValue(name, out var lang))
            return lang;

        var ext = Path.GetExtension(filename);
        return ExtensionMap.TryGetValue(ext, out lang) ? lang : "text";
    }
}