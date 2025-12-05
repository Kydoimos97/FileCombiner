using ReverseMarkdown;

namespace FileCombiner.Modules.Services.TextExtractors.FileTypes;

public class HtmlToMarkdownExtractor : IFileTextExtractor
{
    public bool CanHandle(string ext)
    {
        return ext is ".html" or ".htm";
    }

    public async Task<string> ExtractTextAsync(string path)
    {
        var html = await File.ReadAllTextAsync(path);
        var converter = new Converter(new Config { GithubFlavored = true });
        return converter.Convert(html);
    }
}