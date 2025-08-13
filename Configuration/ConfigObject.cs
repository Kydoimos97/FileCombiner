namespace FileCombiner.Configuration;

using System.Collections;

/// <summary>
/// Base configuration object providing helpers for merging overrides into defaults.
/// </summary>
public abstract class ConfigObject
{
    /// <summary>
    /// Use source if provided; otherwise keep current (value types).
    /// </summary>
    protected static T UseIfProvided<T>(T current, T? source) where T : struct
        => source.HasValue ? source.Value : current;

    /// <summary>
    /// Use source if provided; for strings ignore null/whitespace; for collections require non-empty; else keep current.
    /// </summary>
    protected static T UseIfProvided<T>(T current, T? source) where T : class?
    {
        if (source is null) return current;

        if (source is string s)
            return string.IsNullOrWhiteSpace(s) ? current : source;

        if (source is IEnumerable e)
            return HasAny(e) ? source : current;

        return source;
    }

    /// <summary>
    /// Merge source into current HashSet if source has any items; always returns current.
    /// </summary>
    protected static HashSet<T> UnionIfProvided<T>(HashSet<T> current, IEnumerable<T>? source)
    {
        if (source is null) return current;
        using var en = source.GetEnumerator();
        if (en.MoveNext())
            current.UnionWith(source);
        return current;
    }

    private static bool HasAny(IEnumerable e)
    {
        var en = e.GetEnumerator();
        try { return en.MoveNext(); }
        finally { (en as IDisposable)?.Dispose(); }
    }
}