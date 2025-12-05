using System.Text;
using CommandLine;
using FileCombiner.Modules.Configuration;
using FileCombiner.Modules.Models;
using Spectre.Console;

namespace FileCombiner.Modules.CLI;

/// <summary>
///     Handles CLI help, interactive mode, and output presentation.
/// </summary>
public static class CommandLineInterface
{
    /// <summary>
    ///     Safely escapes text for Spectre.Console markup to prevent parsing errors.
    /// </summary>
    /// <param name="text">The text to escape</param>
    /// <returns>Escaped text safe for markup display</returns>
    public static string SafeMarkup(string text)
    {
        return Markup.Escape(text);
    }

    public static CommandLineOptions? Parse(string[] args)
    {
        // Use CommandLine library's built-in help generation
        var parser = new Parser(with => with.HelpWriter = null);
        var result = parser.ParseArguments<CommandLineOptions>(args);

        CommandLineOptions? opts = null;
        result
            .WithParsed(o => opts = o)
            .WithNotParsed(errors =>
            {
                // Display help using CommandLine library's built-in formatter
                var helpText = CommandLine.Text.HelpText.AutoBuild(result, h =>
                {
                    h.AdditionalNewLineAfterOption = false;
                    h.Heading = "FileCombiner - Combine multiple files into a single reference document";
                    h.Copyright = "";
                    h.AddPreOptionsLine("");
                    h.AddPreOptionsLine("Usage: filecombiner [directory] [options]");
                    h.AddPreOptionsLine("");
                    h.AddPostOptionsLine("");
                    h.AddPostOptionsLine("Examples:");
                    h.AddPostOptionsLine("  filecombiner");
                    h.AddPostOptionsLine("  filecombiner ./src -e .cs,.js -o combined.md");
                    h.AddPostOptionsLine("  filecombiner ./project --exclude \"**/bin/**\" --dry-run");
                    h.MaximumDisplayWidth = 100;
                    return h;
                }, e => e);

                Console.WriteLine(helpText);
            });

        return opts;
    }

    public static CommandLineOptions RunInteractive()
    {
        try
        {
            AnsiConsole.MarkupLine("[bold cyan]🚀 FileCombiner - Interactive Mode[/]");
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[dim]Answer a few questions to configure your file combination...[/]");
            AnsiConsole.WriteLine();

            // Question 1: Source directory
            var dir = AnsiConsole.Ask<string>("📁 [cyan]Source directory to scan[/]:", ".");

            // Question 2: File extensions
            var exts = AnsiConsole.Ask<string>("📝 [cyan]File extensions[/] (comma-separated, or * for all):", "*");

            // Question 3: Output destination
            var outputChoice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("💾 [cyan]Where should the output go?[/]")
                    .AddChoices("Clipboard (default)", "Save to file"));

            string? output = null;
            if (outputChoice == "Save to file")
            {
                output = AnsiConsole.Ask<string>("   [dim]Enter output file path[/]:", "combined.md");
            }

            // Question 4: Preview mode
            var dryRun = AnsiConsole.Confirm("👀 [cyan]Preview mode[/] (show files without processing)?", false);

            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[green]✓[/] Configuration complete!");
            AnsiConsole.WriteLine();

            return new CommandLineOptions
            {
                Directory = dir,
                OutputFile = string.IsNullOrWhiteSpace(output) ? null : output,
                Extensions = exts.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries),
                MaxDepth = 5, // Use sensible default
                DryRun = dryRun,
                Verbose = false, // Use sensible default
                NoTree = false // Use sensible default
            };
        }
        catch (Exception)
        {
            // Fallback to plain console if markup fails
            Console.WriteLine("FileCombiner - Interactive Mode");
            Console.WriteLine();
            Console.WriteLine("Answer a few questions to configure your file combination...");
            Console.WriteLine();

            Console.Write("Source directory to scan [.]: ");
            var dir = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(dir)) dir = ".";

            Console.Write("File extensions (comma-separated, or * for all) [*]: ");
            var exts = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(exts)) exts = "*";

            Console.Write("Output to (1) Clipboard or (2) File? [1]: ");
            var outputChoice = Console.ReadLine();
            string? output = null;
            if (outputChoice == "2")
            {
                Console.Write("Enter output file path [combined.md]: ");
                output = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(output)) output = "combined.md";
            }

            Console.Write("Preview mode (show files without processing)? (y/n) [n]: ");
            var dryRunInput = Console.ReadLine();
            var dryRun = dryRunInput?.ToLower() == "y";

            Console.WriteLine();
            Console.WriteLine("Configuration complete!");
            Console.WriteLine();

            return new CommandLineOptions
            {
                Directory = dir,
                OutputFile = output,
                Extensions = exts.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries),
                MaxDepth = 5,
                DryRun = dryRun,
                Verbose = false,
                NoTree = false
            };
        }
    }

    public static void PrintSummary(ProcessResult result)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold]Summary:[/]");
        AnsiConsole.MarkupLine($"  📁 Directories scanned: [cyan]{result.FoundDirs}[/]");
        AnsiConsole.MarkupLine($"  📄 Files to combine: [yellow]{result.ProcessedFiles.Count}[/]");
        AnsiConsole.MarkupLine($"  📊 Total size: [cyan]{result.TotalSize:N0}[/] bytes");
        AnsiConsole.WriteLine();
    }

    public static async Task OutputResult(CombinedFilesData data, AppConfig config)
    {
        if (string.IsNullOrEmpty(data.FinalContent))
        {
            AnsiConsole.MarkupLine("[yellow]No content to output[/]");
            return;
        }

        if (!string.IsNullOrEmpty(config.OutputFile))
            try
            {
                await File.WriteAllTextAsync(config.OutputFile, data.FinalContent, Encoding.UTF8);
                AnsiConsole.MarkupLine($"[yellow]💾 Saved to:[/] {SafeMarkup(config.OutputFile)}");
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Error saving file:[/] {SafeMarkup(ex.Message)}");
                AnsiConsole.MarkupLine("[yellow]📋 Copying to clipboard instead...[/]");
                await RunTimeUtils.CopyToClipboard(data);
            }
        else
            await RunTimeUtils.CopyToClipboard(data);
    }
}