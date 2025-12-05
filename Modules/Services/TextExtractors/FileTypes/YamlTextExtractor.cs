using System.Text;
using YamlDotNet.RepresentationModel;

namespace FileCombiner.Modules.Services.TextExtractors.FileTypes;

public class YamlTextExtractor : IFileTextExtractor
{
    public bool CanHandle(string ext)
    {
        return ext is ".yaml" or ".yml";
    }

    public async Task<string> ExtractTextAsync(string path)
    {
        try
        {
            var text = await File.ReadAllTextAsync(path);
            var yaml = new YamlStream();
            yaml.Load(new StringReader(text));

            var sb = new StringBuilder();
            for (var i = 0; i < yaml.Documents.Count; i++)
            {
                // build a temp stream containing only one document
                var single = new YamlStream(yaml.Documents[i]);
                await using var writer = new StringWriter();
                single.Save(writer, false);

                sb.AppendLine(writer.ToString().TrimEnd());
                if (i < yaml.Documents.Count - 1)
                    sb.AppendLine("\n---\n");
            }

            return sb.ToString();
        }
        catch
        {
            return await File.ReadAllTextAsync(path);
        }
    }
}