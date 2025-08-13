using Microsoft.Extensions.FileSystemGlobbing;

namespace FileCombiner.Services;

public interface ITextMatcherService
{
    bool IsMatch(string relativePath, bool isDirectory = false);
}


public sealed class FileSystemTextGlobber : ITextMatcherService
{
    private readonly Matcher _include;
    private readonly Matcher _exclude;

    public FileSystemTextGlobber(IEnumerable<string>? includePatterns, IEnumerable<string>? excludePatterns)
    {
        _include = new Matcher(StringComparison.OrdinalIgnoreCase);
        if (includePatterns != null && includePatterns.Any())
            _include.AddIncludePatterns(includePatterns);
        else
            _include.AddInclude("**/*");

        _exclude = new Matcher(StringComparison.OrdinalIgnoreCase);
        if (excludePatterns != null && excludePatterns.Any())
            _exclude.AddExcludePatterns(excludePatterns);
    }

    public bool IsMatch(string relativePath, bool isDirectory = false)
    {
        var p = Normalize(relativePath, isDirectory);
        var inc = _include.Match(new[] { p }).HasMatches;
        if (!inc) return false;
        var exc = _exclude.Match(new[] { p }).HasMatches;
        return !exc;
    }

    public static string Normalize(string path, bool isDirectory)
    {
        // unify separators, strip leading ./ or /
        var s = path.Replace('\\', '/');
        if (s.StartsWith("./", StringComparison.Ordinal)) s = s[2..];
        else if (s.StartsWith("/", StringComparison.Ordinal)) s = s[1..];

        // For dir patterns like "**/bin/**" some matchers behave better with a trailing slash
        if (isDirectory && !s.EndsWith("/", StringComparison.Ordinal)) s += "/";

        return s;
    }
}