using System.Text;
using CommandLine;
using FileCombiner.CLI;
using FileCombiner.Configuration;
using FileCombiner.Models;
using FileCombiner.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Spectre.Console;

namespace FileCombiner;

/// <summary>
/// Main program entry point with dependency injection setup
/// </summary>
class Program
{
    static string Esc(string s) => Markup.Escape(s);

    static async Task<int> Main(string[] args)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        Console.OutputEncoding = Encoding.UTF8;
        // Parse command line arguments
        var parseResult = Parser.Default.ParseArguments<CommandLineOptions>(args);

        return await parseResult.MapResult(
            async options => await RunApplication(options),
            _ => Task.FromResult(1)
        );
    }

    private static async Task<int> RunApplication(CommandLineOptions options)
    {
        try
        {
            // Validate directory
            if (!Directory.Exists(options.Directory))
            {
                AnsiConsole.MarkupLine($"[red]Error:[/] Directory does not exist: {Esc(options.Directory)}");
                return 1;
            }

            // Setup dependency injection
            var services = new ServiceCollection();
            ConfigureServices(services, options);
            // Create configuration
            var config = AppConfig.FromCommandLine(options);

            // Add dependent Service
            AddTextParser(services, config);

            // Build Service Provider
            await using var serviceProvider = services.BuildServiceProvider();

            // Get services
            var discoveryService = serviceProvider.GetRequiredService<IFileDiscoveryService>();
            var combinerService = serviceProvider.GetRequiredService<IFileCombinerService>();

            // STEP 1: Discover files first - show what we found
            AnsiConsole.MarkupLine("[bold]Discovering files...[/]");
            var discoveryResult = await discoveryService.DiscoverFilesAsync(config);

            if (discoveryResult.ProcessedFiles.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]No files found to process.[/]");
                return 0;
            }
            else
            {
                AnsiConsole.WriteLine();
                AnsiConsole.MarkupLine($"[bold]Summary:[/]");
                AnsiConsole.MarkupLine($"  📁 Directories scanned: [cyan]{discoveryResult.FoundDirs}[/]");
                AnsiConsole.MarkupLine($"  📄 Files to combine: [yellow]{discoveryResult.ProcessedFiles.Count}[/]");
                AnsiConsole.MarkupLine($"  📊 Total size: [cyan]{discoveryResult.TotalSize:N0}[/] bytes");
                AnsiConsole.WriteLine();
            }




            if (config.DryRun)
            {
                AnsiConsole.MarkupLine("[yellow]Dry run mode - no files will be processed[/]");
                return 0;
            }

            // STEP 2: Now that user can see what files were found, ask for confirmation
            if (!AnsiConsole.Confirm($"Proceed with combining these {discoveryResult.ProcessedFiles.Count} files?"))
            {
                AnsiConsole.MarkupLine("[yellow]Operation cancelled[/]");
                return 0;
            }
            AnsiConsole.WriteLine();
            // STEP 3: User said yes, now actually combine the files
            AnsiConsole.MarkupLine("[bold]Combining files...[/]");
            var result = await combinerService.CombineFilesAsync(config, discoveryResult);

            // STEP 4: Output result
            await OutputResult(result, config);

            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] {Esc(ex.Message)}");
            if (options.Verbose)
            {
                AnsiConsole.WriteException(ex);
            }
            return 1;
        }
    }

    private static void ConfigureServices(ServiceCollection services, CommandLineOptions options)
    {
        // Logging
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(options.Verbose ? LogLevel.Debug : LogLevel.Information);
        });

        // Application services - Dependency Injection pattern
        services.AddTransient<ITextDetectionService, TextDetectionService>();
        services.AddTransient<IFileDiscoveryService, FileDiscoveryService>();
        services.AddTransient<ILanguageDetectionService, LanguageDetectionService>();
        services.AddTransient<IFileCombinerService, FileCombinerService>();
    }

    private static void AddTextParser(ServiceCollection services, AppConfig config)
    {
        services.AddSingleton<ITextMatcherService>(_ = new FileSystemTextGlobber(config.IncludePatterns, config.ExcludePatterns));
    }

    private static async Task OutputResult(CombinedFilesData data, AppConfig config)
    {
        if (string.IsNullOrEmpty(data.FinalContent))
        {
            AnsiConsole.MarkupLine("[yellow]No content to output[/]");
            return;
        }

        if (!string.IsNullOrEmpty(config.OutputFile))
        {
            try
            {
                await File.WriteAllTextAsync(config.OutputFile, data.FinalContent, Encoding.UTF8);
                AnsiConsole.MarkupLine($"[yellow]💾 Saved to:[/] {config.OutputFile}");
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Error saving file:[/] {Esc(ex.Message)}");
                AnsiConsole.MarkupLine("[yellow]📋 Copying to clipboard instead...[/]");
                await CopyToClipboard(data);
            }
        }
        else
        {
            await CopyToClipboard(data);
        }
    }

    private static async Task CopyToClipboard(CombinedFilesData data)
    {
        try
        {

            await TextCopy.ClipboardService.SetTextAsync(data.FinalContent);
            AnsiConsole.MarkupLine($"📋 Output [yellow]copied[/] to clipboard ([magenta]{data.TotalTokens:N0}[/] Tokens)");
        }
        catch (Exception ex)
        {
            // Fallback to temp file like before
            var tempFile = Path.Combine(Path.GetTempPath(), $"combined-files-{DateTime.Now:yyyyMMdd-HHmmss}.md");
            await File.WriteAllTextAsync(tempFile, data.FinalContent, Encoding.UTF8);

            AnsiConsole.MarkupLine($"[yellow]⚠️ Clipboard failed, saved to:[/] {tempFile}");
            AnsiConsole.MarkupLine($"[dim]Error: {ex.Message}[/]");
        }
    }
}