using FileCombiner.Modules;
using FileCombiner.Modules.CLI;
using FileCombiner.Modules.Configuration;
using FileCombiner.Modules.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FileCombiner.Tests.Services;

/// <summary>
/// Tests for service resolution and initialization
/// </summary>
public class ServiceResolutionTests
{
    // Feature: cli-fixes-and-simplification, Property 2: Service resolution completes without hanging
    /// <summary>
    /// Property test: For any required service type in the DI container,
    /// when that service is resolved, the operation should complete within a reasonable timeout (5 seconds).
    /// Validates: Requirements 2.2, 2.4
    /// </summary>
    [Property(MaxTest = 100)]
    public Property ServiceResolutionCompletesWithinTimeout()
    {
        return Prop.ForAll(
            Arb.Default.Bool(),
            verbose =>
            {
                // Arrange - create service collection with test configuration
                var options = new CommandLineOptions
                {
                    Directory = ".",
                    Verbose = verbose,
                    MaxDepth = 1
                };

                var config = AppConfig.FromCommandLine(options);
                var services = new ServiceCollection();
                RunTimeUtils.ConfigureServices(services, options);
                RunTimeUtils.AddTextParser(services, config);

                using var provider = services.BuildServiceProvider();

                // Act & Assert - resolve each service within timeout
                var serviceTypes = new[]
                {
                    typeof(IFileDiscoveryService),
                    typeof(IFileCombinerService),
                    typeof(ITextDetectionService),
                    typeof(ILanguageDetectionService),
                    typeof(ITextMatcherService)
                };

                foreach (var serviceType in serviceTypes)
                {
                    var resolutionTask = Task.Run(() => provider.GetRequiredService(serviceType));
                    var completed = resolutionTask.Wait(TimeSpan.FromSeconds(5));
                    
                    if (!completed)
                    {
                        return false.ToProperty().Label($"Service {serviceType.Name} resolution timed out");
                    }

                    if (resolutionTask.Result == null)
                    {
                        return false.ToProperty().Label($"Service {serviceType.Name} resolved to null");
                    }
                }

                return true.ToProperty();
            });
    }

    [Fact]
    public void AllRequiredServices_CanBeResolved()
    {
        // Arrange
        var options = new CommandLineOptions
        {
            Directory = ".",
            Verbose = false,
            MaxDepth = 5
        };

        var config = AppConfig.FromCommandLine(options);
        var services = new ServiceCollection();
        RunTimeUtils.ConfigureServices(services, options);
        RunTimeUtils.AddTextParser(services, config);

        using var provider = services.BuildServiceProvider();

        // Act & Assert
        var discoveryService = provider.GetRequiredService<IFileDiscoveryService>();
        Assert.NotNull(discoveryService);

        var combinerService = provider.GetRequiredService<IFileCombinerService>();
        Assert.NotNull(combinerService);

        var textDetectionService = provider.GetRequiredService<ITextDetectionService>();
        Assert.NotNull(textDetectionService);

        var languageService = provider.GetRequiredService<ILanguageDetectionService>();
        Assert.NotNull(languageService);

        var matcherService = provider.GetRequiredService<ITextMatcherService>();
        Assert.NotNull(matcherService);
    }

    [Fact]
    public void ServiceResolution_DoesNotDeadlock()
    {
        // Arrange
        var options = new CommandLineOptions
        {
            Directory = ".",
            Verbose = false
        };

        var config = AppConfig.FromCommandLine(options);
        var services = new ServiceCollection();
        RunTimeUtils.ConfigureServices(services, options);
        RunTimeUtils.AddTextParser(services, config);

        // Act - this should complete quickly without deadlock
        var sw = System.Diagnostics.Stopwatch.StartNew();
        using var provider = services.BuildServiceProvider();
        var discoveryService = provider.GetRequiredService<IFileDiscoveryService>();
        var combinerService = provider.GetRequiredService<IFileCombinerService>();
        sw.Stop();

        // Assert - should complete in under 2 seconds
        Assert.True(sw.ElapsedMilliseconds < 2000, 
            $"Service resolution took {sw.ElapsedMilliseconds}ms, which may indicate a deadlock or performance issue");
        Assert.NotNull(discoveryService);
        Assert.NotNull(combinerService);
    }
}
