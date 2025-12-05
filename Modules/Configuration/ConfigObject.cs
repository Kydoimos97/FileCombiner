using System.Collections;

namespace FileCombiner.Modules.Configuration;

/// <summary>
///     Base configuration object providing helpers for merging overrides into defaults.
/// </summary>
public abstract class ConfigObject
{
    /// <summary>
    ///     Use source if provided; otherwise keep current (value types).
    /// </summary>
    protected static T UseIfProvided<T>(T current, T? source) where T : struct
    {
        return source ?? current;
    }

    /// <summary>
    ///     Use source if provided; for strings ignore null/whitespace; for collections require non-empty; else keep current.
    /// </summary>
    protected static T UseIfProvided<T>(T current, T? source) where T : class?
    {
        return source switch
        {
            null => current,
            string s => string.IsNullOrWhiteSpace(s) ? current : source,
            IEnumerable e => HasAny(e) ? source : current,
            _ => source
        };
    }

    /// <summary>
    ///     Merge source into current HashSet if source has any items; always returns current.
    /// </summary>
    protected static HashSet<T> UnionIfProvided<T>(HashSet<T> current, IEnumerable<T>? source)
    {
        if (source is null)
            return current;

        var items = source as ICollection<T> ?? source.ToArray();
        if (items.Count == 0)
            return current;

        current.UnionWith(items);
        return current;
    }

    private static bool HasAny(IEnumerable e)
    {
        var en = e.GetEnumerator();
        try
        {
            return en.MoveNext();
        }
        finally
        {
            (en as IDisposable)?.Dispose();
        }
    }
}