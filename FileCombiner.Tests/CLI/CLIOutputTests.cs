using FileCombiner.Modules.CLI;
using FileCombiner.Modules.Configuration;
using FileCombiner.Modules.Models;

namespace FileCombiner.Tests.CLI;

/// <summary>
/// Tests for CLI output functionality
/// </summary>
public class CLIOutputTests
{
    [Fact]
    public void PrintSummary_WithValidResult_DoesNotThrow()
    {
        // Arrange
        var files = new List<DiscoveredFile>
        {
            new("test1.cs", "/path/test1.cs", 100, 1),
            new("test2.cs", "/path/test2.cs", 200, 1)
        };
        var result = new ProcessResult(files, new List<string>(), new List<string>(), 300, 0, 5);
        
        // Act & Assert
        var exception = Record.Exception(() => CommandLineInterface.PrintSummary(result));
        Assert.Null(exception);
    }

    [Fact]
    public void PrintSummary_WithEmptyResult_DoesNotThrow()
    {
        // Arrange
        var result = new ProcessResult(new List<DiscoveredFile>(), new List<string>(), new List<string>(), 0, 0, 0);
        
        // Act & Assert
        var exception = Record.Exception(() => CommandLineInterface.PrintSummary(result));
        Assert.Null(exception);
    }

    [Fact]
    public async Task OutputResult_WithEmptyContent_DoesNotThrow()
    {
        // Arrange
        var data = new CombinedFilesData(string.Empty, 0);
        var config = AppConfig.CreateDefault(".");
        
        // Act & Assert
        var exception = await Record.ExceptionAsync(async () => 
            await CommandLineInterface.OutputResult(data, config));
        Assert.Null(exception);
    }

    [Fact]
    public async Task OutputResult_WithValidContentAndFile_WritesFile()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        try
        {
            var data = new CombinedFilesData("Test content", 0);
            var config = AppConfig.CreateDefault(".");
            config.OutputFile = tempFile;
            
            // Act
            await CommandLineInterface.OutputResult(data, config);
            
            // Assert
            Assert.True(File.Exists(tempFile));
            var content = await File.ReadAllTextAsync(tempFile);
            Assert.Equal("Test content", content);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }
}
