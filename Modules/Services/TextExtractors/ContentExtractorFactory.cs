namespace FileCombiner.Modules.Services.TextExtractors;

public class ContentExtractorFactory
{
    private readonly List<IFileTextExtractor> _extractors;

    public ContentExtractorFactory(IEnumerable<IFileTextExtractor> extractors)
    {
        _extractors = extractors.ToList();
    }

    public IFileTextExtractor? GetExtractor(string extension)
    {
        return _extractors.FirstOrDefault(e => e.CanHandle(extension));
    }
}