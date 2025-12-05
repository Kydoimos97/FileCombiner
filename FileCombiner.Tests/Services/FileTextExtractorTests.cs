using FileCombiner.Modules.Services.TextExtractors.FileTypes;

namespace FileCombiner.Tests.Services;

/// <summary>
/// Tests for file text extractor implementations
/// </summary>
public class FileTextExtractorTests
{
    [Fact]
    public void CsvTextExtractor_CanHandle_ReturnsTrueForCsv()
    {
        // Arrange
        var extractor = new CsvTextExtractor();
        
        // Act
        var result = extractor.CanHandle(".csv");
        
        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CsvTextExtractor_CanHandle_ReturnsFalseForOther()
    {
        // Arrange
        var extractor = new CsvTextExtractor();
        
        // Act
        var result = extractor.CanHandle(".txt");
        
        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task CsvTextExtractor_ExtractsBasicCsv()
    {
        // Arrange
        var extractor = new CsvTextExtractor();
        var tempFile = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(tempFile, "Name,Age\nJohn,30\nJane,25");
            
            // Act
            var result = await extractor.ExtractTextAsync(tempFile);
            
            // Assert
            Assert.Contains("Name", result);
            Assert.Contains("John", result);
            Assert.Contains("Jane", result);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public void MarkdownTextExtractor_CanHandle_ReturnsTrueForMarkdown()
    {
        // Arrange
        var extractor = new MarkdownTextExtractor();
        
        // Act
        var resultMd = extractor.CanHandle(".md");
        
        // Assert
        Assert.True(resultMd);
    }

    [Fact]
    public async Task MarkdownTextExtractor_ExtractsMarkdown()
    {
        // Arrange
        var extractor = new MarkdownTextExtractor();
        var tempFile = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(tempFile, "# Header\n\nSome **bold** text.");
            
            // Act
            var result = await extractor.ExtractTextAsync(tempFile);
            
            // Assert
            Assert.Contains("Header", result);
            Assert.Contains("bold", result);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public void YamlTextExtractor_CanHandle_ReturnsTrueForYaml()
    {
        // Arrange
        var extractor = new YamlTextExtractor();
        
        // Act
        var resultYml = extractor.CanHandle(".yml");
        var resultYaml = extractor.CanHandle(".yaml");
        
        // Assert
        Assert.True(resultYml);
        Assert.True(resultYaml);
    }

    [Fact]
    public async Task YamlTextExtractor_ExtractsYaml()
    {
        // Arrange
        var extractor = new YamlTextExtractor();
        var tempFile = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(tempFile, "name: John\nage: 30");
            
            // Act
            var result = await extractor.ExtractTextAsync(tempFile);
            
            // Assert
            Assert.Contains("name", result);
            Assert.Contains("John", result);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public void HtmlToMarkdownExtractor_CanHandle_ReturnsTrueForHtml()
    {
        // Arrange
        var extractor = new HtmlToMarkdownExtractor();
        
        // Act
        var resultHtml = extractor.CanHandle(".html");
        var resultHtm = extractor.CanHandle(".htm");
        
        // Assert
        Assert.True(resultHtml);
        Assert.True(resultHtm);
    }

    [Fact]
    public async Task HtmlToMarkdownExtractor_ExtractsHtml()
    {
        // Arrange
        var extractor = new HtmlToMarkdownExtractor();
        var tempFile = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(tempFile, "<html><body><h1>Title</h1><p>Content</p></body></html>");
            
            // Act
            var result = await extractor.ExtractTextAsync(tempFile);
            
            // Assert
            Assert.Contains("Title", result);
            Assert.Contains("Content", result);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public void DocxTextExtractor_CanHandle_ReturnsTrueForDocx()
    {
        // Arrange
        var extractor = new DocxTextExtractor();
        
        // Act
        var result = extractor.CanHandle(".docx");
        
        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ExcelTextExtractor_CanHandle_ReturnsTrueForExcel()
    {
        // Arrange
        var extractor = new ExcelTextExtractor();
        
        // Act
        var resultXlsx = extractor.CanHandle(".xlsx");
        var resultXls = extractor.CanHandle(".xls");
        
        // Assert
        Assert.True(resultXlsx);
        Assert.True(resultXls);
    }

    [Fact]
    public void PdfTextExtractor_CanHandle_ReturnsTrueForPdf()
    {
        // Arrange
        var extractor = new PdfTextExtractor();
        
        // Act
        var result = extractor.CanHandle(".pdf");
        
        // Assert
        Assert.True(result);
    }

    [Fact]
    public void PptxTextExtractor_CanHandle_ReturnsTrueForPptx()
    {
        // Arrange
        var extractor = new PptxTextExtractor();
        
        // Act
        var result = extractor.CanHandle(".pptx");
        
        // Assert
        Assert.True(result);
    }
}
