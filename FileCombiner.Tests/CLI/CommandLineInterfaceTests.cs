using FileCombiner.Modules.CLI;
using Spectre.Console;

namespace FileCombiner.Tests.CLI;

/// <summary>
/// Tests for CommandLineInterface functionality
/// </summary>
public class CommandLineInterfaceTests
{
    // Feature: cli-fixes-and-simplification, Property 1: Text escaping prevents markup errors
    /// <summary>
    /// Property test: For any string containing potential Spectre.Console markup keywords,
    /// when that string is escaped and displayed, the system should not throw markup parsing exceptions.
    /// Validates: Requirements 1.2
    /// </summary>
    [Property(MaxTest = 100)]
    public Property EscapedTextNeverThrowsMarkupException(string arbitraryText)
    {
        // Handle null input
        if (arbitraryText == null)
        {
            arbitraryText = string.Empty;
        }

        return Prop.ForAll(
            Arb.Default.String(),
            text =>
            {
                text ??= string.Empty;
                
                // Escape the text using SafeMarkup
                var escaped = CommandLineInterface.SafeMarkup(text);
                
                // Try to render the escaped text - this should never throw
                var exception = Record.Exception(() =>
                {
                    // Use Spectre.Console's internal markup parser to validate
                    var markup = new Markup(escaped);
                    // Force evaluation by getting the segments
                    _ = markup.ToString();
                });
                
                return exception == null;
            });
    }

    [Fact]
    public void SafeMarkup_EscapesSquareBrackets()
    {
        // Arrange
        var textWithBrackets = "[red]This should be escaped[/]";
        
        // Act
        var escaped = CommandLineInterface.SafeMarkup(textWithBrackets);
        
        // Assert - brackets should be doubled to escape them
        Assert.Contains("[[", escaped);
        Assert.Contains("]]", escaped);
        // And it should not throw when creating Markup
        var exception = Record.Exception(() => new Markup(escaped));
        Assert.Null(exception);
    }

    [Fact]
    public void SafeMarkup_HandlesEmptyString()
    {
        // Arrange
        var empty = string.Empty;
        
        // Act
        var escaped = CommandLineInterface.SafeMarkup(empty);
        
        // Assert
        Assert.Equal(string.Empty, escaped);
    }

    [Theory]
    [InlineData("[bold]text[/]")]
    [InlineData("[red]error[/]")]
    [InlineData("[yellow]warning[/]")]
    [InlineData("[green]success[/]")]
    [InlineData("[[escaped]]")]
    public void SafeMarkup_EscapesCommonMarkupPatterns(string input)
    {
        // Act
        var escaped = CommandLineInterface.SafeMarkup(input);
        
        // Assert - escaped text should not throw when creating Markup
        var exception = Record.Exception(() => new Markup(escaped));
        Assert.Null(exception);
    }
}
