using Microsoft.Extensions.FileSystemGlobbing;

namespace FileCombiner.Modules.Services;

public interface ITextMatcherService
{
    bool IsMatch(string relativePath, bool isDirectory = false);
}

public sealed class FileSystemTextGlobber : ITextMatcherService
{
    private readonly Matcher _exclude;
    private readonly Matcher _include;

    public FileSystemTextGlobber(IEnumerable<string>? includePatterns, IEnumerable<string>? excludePatterns)
    {
        _include = new Matcher(StringComparison.OrdinalIgnoreCase);
        var includes = includePatterns?.ToArray() ?? [];
        if (includes.Length > 0)
            _include.AddIncludePatterns(includes);
        else
            _include.AddInclude("**/*");

        _exclude = new Matcher(StringComparison.OrdinalIgnoreCase);
        var excludes = excludePatterns?.ToArray() ?? [];
        if (excludes.Length > 0)
            _exclude.AddExcludePatterns(excludes);
    }

    public bool IsMatch(string relativePath, bool isDirectory = false)
    {
        var p = Normalize(relativePath, isDirectory);
        var inc = _include.Match([p]).HasMatches;
        if (!inc) return false;
        var exc = _exclude.Match([p]).HasMatches;
        return !exc;
    }

    private static string Normalize(string path, bool isDirectory)
    {
        // unify separators, strip leading ./ or /
        var s = path.Replace('\\', '/');
        if (s.StartsWith("./", StringComparison.Ordinal)) s = s[2..];
        else if (s.StartsWith('/')) s = s[1..];

        // For dir patterns like "**/bin/**" some matchers behave better with a trailing slash
        if (isDirectory && !s.EndsWith('/')) s += "/";

        return s;
    }
}