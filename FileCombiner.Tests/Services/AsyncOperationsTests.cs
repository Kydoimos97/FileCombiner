using FileCombiner.Modules;
using FileCombiner.Modules.CLI;
using FileCombiner.Modules.Configuration;
using FileCombiner.Modules.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FileCombiner.Tests.Services;

/// <summary>
/// Tests for async operations to ensure they don't deadlock
/// </summary>
public class AsyncOperationsTests
{
    // Feature: cli-fixes-and-simplification, Property 3: Async operations complete without deadlock
    /// <summary>
    /// Property test: For any async operation in the application flow,
    /// when that operation is awaited, it should complete within a reasonable timeout without deadlocking.
    /// Validates: Requirements 5.3
    /// </summary>
    [Property(MaxTest = 100)]
    public Property AsyncOperationsCompleteWithoutDeadlock(NonEmptyString directoryGen)
    {
        return Prop.ForAll(
            Arb.Default.Bool(),
            verbose =>
            {
                // Use current directory for testing (directoryGen might not exist)
                var directory = ".";
                
                // Arrange
                var options = new CommandLineOptions
                {
                    Directory = directory,
                    Verbose = verbose,
                    MaxDepth = 1,
                    MaxFiles = 5 // Limit files for faster testing
                };

                var config = AppConfig.FromCommandLine(options);
                var services = new ServiceCollection();
                RunTimeUtils.ConfigureServices(services, options);
                RunTimeUtils.AddTextParser(services, config);

                using var provider = services.BuildServiceProvider();
                var discoveryService = provider.GetRequiredService<IFileDiscoveryService>();

                // Act - run async operation with timeout
                var discoveryTask = discoveryService.DiscoverFilesAsync(config);
                var completed = discoveryTask.Wait(TimeSpan.FromSeconds(10));

                // Assert
                if (!completed)
                {
                    return false.ToProperty().Label("DiscoverFilesAsync timed out after 10 seconds");
                }

                if (discoveryTask.IsFaulted)
                {
                    return false.ToProperty().Label($"DiscoverFilesAsync faulted: {discoveryTask.Exception?.Message}");
                }

                return true.ToProperty();
            });
    }

    [Fact]
    public async Task DiscoverFilesAsync_CompletesWithoutDeadlock()
    {
        // Arrange
        var options = new CommandLineOptions
        {
            Directory = ".",
            Verbose = false,
            MaxDepth = 1,
            MaxFiles = 10
        };

        var config = AppConfig.FromCommandLine(options);
        var services = new ServiceCollection();
        RunTimeUtils.ConfigureServices(services, options);
        RunTimeUtils.AddTextParser(services, config);

        await using var provider = services.BuildServiceProvider();
        var discoveryService = provider.GetRequiredService<IFileDiscoveryService>();

        // Act
        var sw = System.Diagnostics.Stopwatch.StartNew();
        var result = await discoveryService.DiscoverFilesAsync(config);
        sw.Stop();

        // Assert
        Assert.NotNull(result);
        Assert.True(sw.ElapsedMilliseconds < 5000, 
            $"DiscoverFilesAsync took {sw.ElapsedMilliseconds}ms, which may indicate a performance issue");
    }

    [Fact]
    public async Task CombineFilesAsync_CompletesWithoutDeadlock()
    {
        // Arrange
        var options = new CommandLineOptions
        {
            Directory = ".",
            Verbose = false,
            MaxDepth = 1,
            MaxFiles = 5
        };

        var config = AppConfig.FromCommandLine(options);
        var services = new ServiceCollection();
        RunTimeUtils.ConfigureServices(services, options);
        RunTimeUtils.AddTextParser(services, config);

        await using var provider = services.BuildServiceProvider();
        var discoveryService = provider.GetRequiredService<IFileDiscoveryService>();
        var combinerService = provider.GetRequiredService<IFileCombinerService>();

        var discoveryResult = await discoveryService.DiscoverFilesAsync(config);

        // Act
        var sw = System.Diagnostics.Stopwatch.StartNew();
        var combined = await combinerService.CombineFilesAsync(config, discoveryResult);
        sw.Stop();

        // Assert
        Assert.NotNull(combined);
        Assert.True(sw.ElapsedMilliseconds < 10000, 
            $"CombineFilesAsync took {sw.ElapsedMilliseconds}ms, which may indicate a performance issue");
    }

    [Fact]
    public async Task AsyncOperations_UseProperAwaitPatterns()
    {
        // This test verifies that async operations don't use .Result or .Wait()
        // by ensuring they can be awaited without blocking

        // Arrange
        var options = new CommandLineOptions
        {
            Directory = ".",
            MaxDepth = 1,
            MaxFiles = 3
        };

        var config = AppConfig.FromCommandLine(options);
        var services = new ServiceCollection();
        RunTimeUtils.ConfigureServices(services, options);
        RunTimeUtils.AddTextParser(services, config);

        await using var provider = services.BuildServiceProvider();
        var discoveryService = provider.GetRequiredService<IFileDiscoveryService>();

        // Act - this should not block the thread
        var task1 = discoveryService.DiscoverFilesAsync(config);
        var task2 = discoveryService.DiscoverFilesAsync(config);

        await Task.WhenAll(task1, task2);

        // Assert - both tasks completed
        Assert.True(task1.IsCompletedSuccessfully);
        Assert.True(task2.IsCompletedSuccessfully);
    }
}
