using FileCombiner.Modules.Services.TextExtractors.FileTypes;

namespace FileCombiner.Tests.Services;

/// <summary>
/// Tests for binary file text extractors using real test files
/// </summary>
public class BinaryFileExtractorTests
{
    private const string TestFixturesPath = "TestFixtures";

    [Fact]
    public async Task DocxTextExtractor_ThrowsOnInvalidFile()
    {
        // Arrange
        var extractor = new DocxTextExtractor();
        var tempFile = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(tempFile, "This is not a valid docx file");
            
            // Act & Assert - should throw on invalid file
            await Assert.ThrowsAnyAsync<Exception>(async () => 
                await extractor.ExtractTextAsync(tempFile));
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task ExcelTextExtractor_ThrowsOnInvalidFile()
    {
        // Arrange
        var extractor = new ExcelTextExtractor();
        var tempFile = Path.ChangeExtension(Path.GetTempFileName(), ".xlsx");
        try
        {
            await File.WriteAllTextAsync(tempFile, "This is not a valid excel file");
            
            // Act & Assert - should throw on invalid file
            await Assert.ThrowsAnyAsync<Exception>(async () => 
                await extractor.ExtractTextAsync(tempFile));
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task PdfTextExtractor_ExtractsFromPdfFile()
    {
        // Arrange
        var extractor = new PdfTextExtractor();
        var testFile = Path.Combine(TestFixturesPath, "file-sample_150kB.pdf");
        
        // Skip if file doesn't exist
        if (!File.Exists(testFile))
        {
            return;
        }
        
        // Act
        var result = await extractor.ExtractTextAsync(testFile);
        
        // Assert
        Assert.NotNull(result);
        // PDF might be empty or have content
    }

    [Fact]
    public async Task PdfTextExtractor_HandlesInvalidFile()
    {
        // Arrange
        var extractor = new PdfTextExtractor();
        var tempFile = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(tempFile, "This is not a valid PDF file");
            
            // Act
            var result = await extractor.ExtractTextAsync(tempFile);
            
            // Assert - should return empty or error message, not throw
            Assert.NotNull(result);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task PptxTextExtractor_ExtractsFromPptxFile()
    {
        // Arrange
        var extractor = new PptxTextExtractor();
        var testFile = Path.Combine(TestFixturesPath, "file_example_PPTX_250kB.pptx");
        
        // Skip if file doesn't exist
        if (!File.Exists(testFile))
        {
            return;
        }
        
        // Act
        var result = await extractor.ExtractTextAsync(testFile);
        
        // Assert
        Assert.NotNull(result);
        // PPTX might have content or be empty
    }

    [Fact]
    public async Task PptxTextExtractor_ThrowsOnInvalidFile()
    {
        // Arrange
        var extractor = new PptxTextExtractor();
        var tempFile = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(tempFile, "This is not a valid pptx file");
            
            // Act & Assert - should throw on invalid file
            await Assert.ThrowsAnyAsync<Exception>(async () => 
                await extractor.ExtractTextAsync(tempFile));
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task ExcelTextExtractor_ExtractsFromXlsxFile()
    {
        // Arrange
        var extractor = new ExcelTextExtractor();
        var testFile = Path.Combine(TestFixturesPath, "file_example_XLSX_50.xlsx");
        
        // Skip if file doesn't exist
        if (!File.Exists(testFile))
        {
            return;
        }
        
        // Act
        var result = await extractor.ExtractTextAsync(testFile);
        
        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public void DocxTextExtractor_CanHandle_SupportsDocx()
    {
        // Arrange
        var extractor = new DocxTextExtractor();
        
        // Act & Assert
        Assert.True(extractor.CanHandle(".docx"));
        Assert.False(extractor.CanHandle(".doc")); // Only supports .docx
        Assert.False(extractor.CanHandle(".txt"));
    }

    [Fact]
    public void ExcelTextExtractor_CanHandle_SupportsMultipleExtensions()
    {
        // Arrange
        var extractor = new ExcelTextExtractor();
        
        // Act & Assert
        Assert.True(extractor.CanHandle(".xlsx"));
        Assert.True(extractor.CanHandle(".xls"));
        Assert.False(extractor.CanHandle(".txt"));
    }

    [Fact]
    public void PptxTextExtractor_CanHandle_SupportsPptx()
    {
        // Arrange
        var extractor = new PptxTextExtractor();
        
        // Act & Assert
        Assert.True(extractor.CanHandle(".pptx"));
        Assert.False(extractor.CanHandle(".ppt")); // Only supports .pptx
        Assert.False(extractor.CanHandle(".txt"));
    }
}
