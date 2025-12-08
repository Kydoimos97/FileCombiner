using System.Diagnostics.CodeAnalysis;
using System.Text;
using FileCombiner.Modules;
using FileCombiner.Modules.CLI;
using FileCombiner.Modules.Configuration;
using FileCombiner.Modules.Services;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;

namespace FileCombiner;

/// <summary>
///     Main program entry point with dependency injection and CLI orchestration.
/// </summary>
// ReSharper disable once ClassNeverInstantiated.Global
[ExcludeFromCodeCoverage] // Entry point - not unit testable
internal class Program
{
    private static string Esc(string s)
    {
        return Markup.Escape(s);
    }

    /// <summary>
    ///     Wraps an async operation with timeout detection to diagnose hangs.
    /// </summary>
    private static async Task<T> WithTimeoutDetection<T>(Task<T> task, string operationName, int timeoutSeconds = 30)
    {
        var timeoutTask = Task.Delay(TimeSpan.FromSeconds(timeoutSeconds));
        var completedTask = await Task.WhenAny(task, timeoutTask);
        
        if (completedTask == timeoutTask)
        {
            AnsiConsole.MarkupLine($"[red]WARNING:[/] Operation '{operationName}' has been running for {timeoutSeconds} seconds...");
            AnsiConsole.MarkupLine("[yellow]This may indicate a deadlock or hang. Waiting for completion...[/]");
            return await task; // Continue waiting but user is informed
        }
        
        return await task;
    }

    private static async Task<int> Main(string[] args)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        Console.OutputEncoding = Encoding.UTF8;

        // Parse command line options
        var options = CommandLineInterface.Parse(args);
        if (options == null) return 1;

        // If interactive mode is requested, run interactive prompts with timeout
        if (options.Interactive)
        {
            var interactiveTask = Task.Run(() => CommandLineInterface.RunInteractive());
            var timeoutTask = Task.Delay(TimeSpan.FromSeconds(60));
            var completedTask = await Task.WhenAny(interactiveTask, timeoutTask);
            
            if (completedTask == timeoutTask)
            {
                AnsiConsole.MarkupLine("[yellow]Interactive mode timed out after 60 seconds. Exiting...[/]");
                return 0;
            }
            
            options = await interactiveTask;
        }

        try
        {
            // Validate directory
            if (!Directory.Exists(options.Directory))
            {
                AnsiConsole.MarkupLine($"[red]Error:[/] Directory does not exist: {Esc(options.Directory)}");
                return 1;
            }

            // Build app configuration from CLI
            if (options.Verbose)
                AnsiConsole.MarkupLine("[dim]DEBUG: Building app configuration from command line options...[/]");
            
            var config = AppConfig.FromCommandLine(options);
            
            if (options.Verbose)
                AnsiConsole.MarkupLine("[dim]DEBUG: App configuration created successfully[/]");

            // Setup dependency injection
            if (options.Verbose)
                AnsiConsole.MarkupLine("[dim]DEBUG: Setting up dependency injection container...[/]");
            
            var services = new ServiceCollection();
            
            if (options.Verbose)
                AnsiConsole.MarkupLine("[dim]DEBUG: Configuring services...[/]");
            
            RunTimeUtils.ConfigureServices(services, options);
            
            if (options.Verbose)
                AnsiConsole.MarkupLine("[dim]DEBUG: Adding text parser services...[/]");
            
            RunTimeUtils.AddTextParser(services, config);

            if (options.Verbose)
                AnsiConsole.MarkupLine("[dim]DEBUG: Building service provider...[/]");
            else
                AnsiConsole.MarkupLine("[grey]Building service provider...[/]");
            
            await using var provider = services.BuildServiceProvider();
            
            if (options.Verbose)
                AnsiConsole.MarkupLine("[dim]DEBUG: Service provider built successfully[/]");
            else
                AnsiConsole.MarkupLine("[grey]Service provider built.[/]");

            if (options.Verbose)
                AnsiConsole.MarkupLine("[dim]DEBUG: Resolving IFileDiscoveryService...[/]");
            
            var discoveryService = await Task.Run(() => provider.GetRequiredService<IFileDiscoveryService>());
            
            if (options.Verbose)
                AnsiConsole.MarkupLine("[dim]DEBUG: IFileDiscoveryService resolved successfully[/]");
            
            if (options.Verbose)
                AnsiConsole.MarkupLine("[dim]DEBUG: Resolving IFileCombinerService...[/]");
            
            var combinerService = await Task.Run(() => provider.GetRequiredService<IFileCombinerService>());
            
            if (options.Verbose)
                AnsiConsole.MarkupLine("[dim]DEBUG: IFileCombinerService resolved successfully[/]");
            else
                AnsiConsole.MarkupLine("[grey]Services resolved successfully.[/]");

            if (options.Verbose)
                AnsiConsole.MarkupLine("[dim]DEBUG: Starting file discovery...[/]");
            else
                AnsiConsole.MarkupLine("[grey]Starting discovery...[/]");

            // STEP 1: Discover files
            AnsiConsole.MarkupLine("[bold]Discovering files...[/]");
            
            if (options.Verbose)
                AnsiConsole.MarkupLine("[dim]DEBUG: Calling DiscoverFilesAsync...[/]");
            
            var discoveryResult = await WithTimeoutDetection(
                discoveryService.DiscoverFilesAsync(config),
                "File Discovery",
                30
            ).ConfigureAwait(false);
            
            if (options.Verbose)
                AnsiConsole.MarkupLine("[dim]DEBUG: DiscoverFilesAsync completed successfully[/]");
            else
                AnsiConsole.MarkupLine("[grey]Finished discovery...[/]");


            if (discoveryResult.ProcessedFiles.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]No files found to process.[/]");
                return 0;
            }

            // STEP 2: Print summary
            CommandLineInterface.PrintSummary(discoveryResult);

            if (config.DryRun)
            {
                AnsiConsole.MarkupLine("[yellow]Dry run mode - no files will be processed[/]");
                return 0;
            }

            // Confirm action
            if (!await AnsiConsole.ConfirmAsync($"Proceed with combining these {discoveryResult.ProcessedFiles.Count} files?"))
            {
                AnsiConsole.MarkupLine("[yellow]Operation cancelled[/]");
                return 0;
            }

            AnsiConsole.WriteLine();

            // STEP 3: Combine files
            AnsiConsole.MarkupLine("[bold]Combining files...[/]");
            
            if (options.Verbose)
                AnsiConsole.MarkupLine("[dim]DEBUG: Calling CombineFilesAsync...[/]");
            
            var combined = await WithTimeoutDetection(
                combinerService.CombineFilesAsync(config, discoveryResult),
                "File Combining",
                60
            ).ConfigureAwait(false);
            
            if (options.Verbose)
                AnsiConsole.MarkupLine("[dim]DEBUG: CombineFilesAsync completed successfully[/]");

            // STEP 4: Output result (delegated to CLI)
            await CommandLineInterface.OutputResult(combined, config);

            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] {Esc(ex.Message)}");
            AnsiConsole.WriteException(ex, ExceptionFormats.ShortenPaths | ExceptionFormats.ShortenTypes);
            return 1;
        }
    }
}