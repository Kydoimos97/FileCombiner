using FileCombiner.Modules.CLI;
using FileCombiner.Modules.Configuration;
using FileCombiner.Modules.Services;
using FsCheck;
using FsCheck.Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace FileCombiner.Tests.Services;

public class PathResolutionTests
{
    // Feature: cli-fixes-and-simplification, Property 5: Path resolution is location-independent
    [Property(MaxTest = 100)]
    public Property PathResolutionIsLocationIndependent()
    {
        // Generate valid relative directory paths
        var validPaths = Gen.Elements(".", "./Modules", "Modules", "Modules/CLI", "./FileCombiner.Tests")
            .ToArbitrary();

        return Prop.ForAll(validPaths, path =>
        {
            try
            {
                // Create a service provider
                var services = new ServiceCollection();
                services.AddLogging(builder => builder.SetMinimumLevel(LogLevel.Critical));
                
                var opts = new CommandLineOptions { Directory = path, Verbose = false };
                Modules.RunTimeUtils.ConfigureServices(services, opts);
                
                var config = AppConfig.CreateDefault(path);
                Modules.RunTimeUtils.AddTextParser(services, config);
                
                var provider = services.BuildServiceProvider();

                // Resolve the discovery service
                var discoveryService = provider.GetRequiredService<IFileDiscoveryService>();

                // Verify the path is resolved relative to current directory
                // The key test: this should work regardless of where the project is located
                var absolutePath = Path.GetFullPath(path);

                // The path should be resolvable and not contain hardcoded locations
                var exists = Directory.Exists(absolutePath);

                // If the directory exists, discovery should work
                if (exists)
                {
                    var task = discoveryService.DiscoverFilesAsync(config);
                    var completed = task.Wait(TimeSpan.FromSeconds(5));
                    return completed.ToProperty().Label("Discovery completed within timeout");
                }

                // If directory doesn't exist, that's fine - we're testing path resolution, not existence
                return true.ToProperty().Label("Path resolved successfully");
            }
            catch (Exception ex)
            {
                return false.ToProperty().Label($"Path resolution failed: {ex.Message}");
            }
        });
    }

    [Fact]
    public void CurrentDirectoryPathResolvesCorrectly()
    {
        var path = ".";
        var absolutePath = Path.GetFullPath(path);

        // Should resolve to current directory
        Assert.True(Directory.Exists(absolutePath));
        Assert.Equal(Directory.GetCurrentDirectory(), absolutePath);
    }

    [Fact]
    public void RelativePathResolvesCorrectly()
    {
        // Find the solution root by looking for the .sln file
        var currentDir = Directory.GetCurrentDirectory();
        var solutionRoot = currentDir;
        
        while (!File.Exists(Path.Combine(solutionRoot, "FileCombiner.sln")) && Directory.GetParent(solutionRoot) != null)
        {
            solutionRoot = Directory.GetParent(solutionRoot)!.FullName;
        }
        
        var modulesPath = Path.Combine(solutionRoot, "Modules");
        
        // Should resolve and exist
        Assert.True(Directory.Exists(modulesPath), $"Modules directory should exist at {modulesPath}");
        Assert.Contains("Modules", modulesPath);
    }

    [Fact]
    public void PathWithoutPrefixResolvesCorrectly()
    {
        // Test that Path.GetFullPath resolves relative to current directory
        var currentDir = Directory.GetCurrentDirectory();
        var path = "TestSubDir";
        var absolutePath = Path.GetFullPath(path);

        // Should resolve relative to current directory
        Assert.StartsWith(currentDir, absolutePath);
        Assert.EndsWith("TestSubDir", absolutePath);
    }

    [Fact]
    public void NoHardcodedPathsInProductionCode()
    {
        // Find the solution root
        var currentDir = Directory.GetCurrentDirectory();
        var solutionRoot = currentDir;
        
        while (!File.Exists(Path.Combine(solutionRoot, "FileCombiner.sln")) && Directory.GetParent(solutionRoot) != null)
        {
            solutionRoot = Directory.GetParent(solutionRoot)!.FullName;
        }
        
        // This test verifies that production source files don't contain hardcoded absolute paths
        var modulesPath = Path.Combine(solutionRoot, "Modules");
        var programFile = Path.Combine(solutionRoot, "Program.cs");
        
        var sourceFiles = Directory.Exists(modulesPath) 
            ? Directory.GetFiles(modulesPath, "*.cs", SearchOption.AllDirectories).ToList()
            : new List<string>();
            
        if (File.Exists(programFile))
        {
            sourceFiles.Add(programFile);
        }

        Assert.NotEmpty(sourceFiles);

        foreach (var file in sourceFiles)
        {
            var content = File.ReadAllText(file);

            // Check for common hardcoded path patterns (but not in comments or strings that are examples)
            // We're looking for actual hardcoded paths in code, not in test assertions
            var lines = content.Split('\n');
            foreach (var line in lines)
            {
                // Skip comments and using statements
                var trimmed = line.Trim();
                if (trimmed.StartsWith("//") || trimmed.StartsWith("using "))
                    continue;

                // Check for actual hardcoded Windows paths
                if (trimmed.Contains("C:\\Users\\") || trimmed.Contains("D:\\"))
                {
                    Assert.Fail($"Found hardcoded Windows path in {file}: {line}");
                }
            }
        }
    }
}
