using FileCombiner.Modules.Configuration;
using FileCombiner.Modules.Models;

namespace FileCombiner.Tests.Models;

/// <summary>
/// Tests for record model classes
/// </summary>
public class RecordModelTests
{
    [Fact]
    public void ProcessResult_Constructor_InitializesProperties()
    {
        // Arrange
        var files = new List<DiscoveredFile>
        {
            new("test.cs", "/path/test.cs", 100, 1)
        };
        var skippedFiles = new List<string> { "skip1.txt" };
        var skippedDirs = new List<string> { "node_modules" };
        
        // Act
        var result = new ProcessResult(files, skippedFiles, skippedDirs, 1024, 50, 5);
        
        // Assert
        Assert.Single(result.ProcessedFiles);
        Assert.Single(result.SkippedFiles);
        Assert.Single(result.SkippedDirectories);
        Assert.Equal(1024, result.TotalSize);
        Assert.Equal(50, result.TokenCount);
        Assert.Equal(5, result.FoundDirs);
    }

    [Fact]
    public void DiscoveredFile_Properties_SetCorrectly()
    {
        // Arrange & Act
        var file = new DiscoveredFile("src/test.cs", "/project/src/test.cs", 2048, 2);
        
        // Assert
        Assert.Equal("src/test.cs", file.RelativePath);
        Assert.Equal("/project/src/test.cs", file.AbsolutePath);
        Assert.Equal(2048, file.Size);
        Assert.Equal(2, file.Depth);
        Assert.Equal(".cs", file.Extension);
        Assert.Equal("test.cs", file.Name);
    }

    [Fact]
    public void DiscoveredFile_Extension_ReturnsLowerCase()
    {
        // Arrange & Act
        var file = new DiscoveredFile("Test.CS", "/path/Test.CS", 100, 1);
        
        // Assert
        Assert.Equal(".cs", file.Extension);
    }

    [Fact]
    public void DiscoveredFile_Name_ReturnsFileName()
    {
        // Arrange & Act
        var file = new DiscoveredFile("src/subfolder/test.cs", "/path/src/subfolder/test.cs", 100, 2);
        
        // Assert
        Assert.Equal("test.cs", file.Name);
    }

    [Fact]
    public void CombinedFilesData_Constructor_InitializesProperties()
    {
        // Arrange & Act
        var data = new CombinedFilesData("test content", 100);
        
        // Assert
        Assert.Equal("test content", data.FinalContent);
        Assert.Equal(100, data.TotalTokens);
    }

    [Fact]
    public void AppConfig_CreateDefault_InitializesWithDefaults()
    {
        // Arrange & Act
        var config = AppConfig.CreateDefault("./src");
        
        // Assert
        Assert.Equal("./src", config.Directory);
        Assert.Equal(5, config.MaxDepth);
        Assert.True(config.IncludeTree);
        Assert.False(config.DryRun);
        Assert.False(config.Verbose);
        Assert.NotEmpty(config.IgnoreDirs);
        Assert.Contains("node_modules", config.IgnoreDirs);
        Assert.Contains(".git", config.IgnoreDirs);
    }

    [Fact]
    public void AppConfig_FromCommandLine_MapsCorrectly()
    {
        // Arrange
        var options = new Modules.CLI.CommandLineOptions
        {
            Directory = "./src",
            Extensions = new[] { ".cs", ".js" },
            OutputFile = "output.md",
            MaxDepth = 3,
            DryRun = true,
            Verbose = true,
            NoTree = true,
            ExcludePatterns = new[] { "**/bin/**" }
        };
        
        // Act
        var config = AppConfig.FromCommandLine(options);
        
        // Assert
        Assert.Equal("./src", config.Directory);
        Assert.Contains(".cs", config.Extensions);
        Assert.Contains(".js", config.Extensions);
        Assert.Equal("output.md", config.OutputFile);
        Assert.Equal(3, config.MaxDepth);
        Assert.True(config.DryRun);
        Assert.True(config.Verbose);
        Assert.False(config.IncludeTree); // NoTree = true means IncludeTree = false
        Assert.Contains("**/bin/**", config.ExcludePatterns);
    }

    [Fact]
    public void AppConfig_FromCommandLine_HandlesNullExtensions()
    {
        // Arrange
        var options = new Modules.CLI.CommandLineOptions
        {
            Directory = "./src",
            Extensions = null
        };
        
        // Act
        var config = AppConfig.FromCommandLine(options);
        
        // Assert
        Assert.NotNull(config.Extensions);
        Assert.Single(config.Extensions);
        Assert.Equal("*", config.Extensions[0]);
    }

    [Fact]
    public void AppConfig_AutoDetectText_ReturnsTrueForWildcard()
    {
        // Arrange
        var config = AppConfig.CreateDefault(".");
        config.Extensions = new List<string> { "*" };
        
        // Act
        var result = config.AutoDetectText;
        
        // Assert
        Assert.True(result);
    }

    [Fact]
    public void AppConfig_AutoDetectText_ReturnsFalseForSpecificExtensions()
    {
        // Arrange
        var config = AppConfig.CreateDefault(".");
        config.Extensions = new List<string> { ".cs", ".js" };
        
        // Act
        var result = config.AutoDetectText;
        
        // Assert
        Assert.False(result);
    }
}
