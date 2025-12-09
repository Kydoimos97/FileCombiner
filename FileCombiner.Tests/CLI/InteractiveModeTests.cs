using FileCombiner.Modules.CLI;

namespace FileCombiner.Tests.CLI;

/// <summary>
/// Tests for interactive mode behavior
/// </summary>
public class InteractiveModeTests
{
    // Feature: cli-fixes-and-simplification, Property 4: Arguments prevent interactive mode
    /// <summary>
    /// Test: When the application is started with a directory argument,
    /// interactive mode should not be entered.
    /// Validates: Requirements 6.4
    /// </summary>
    [Fact]
    public void ArgumentsPreventInteractiveMode()
    {
        // Arrange - simple directory argument
        var args = new[] { "." };

        // Act
        var options = CommandLineInterface.Parse(args);

        // Assert - should not enter interactive mode
        Assert.NotNull(options);
        Assert.False(options.Interactive);
    }

    [Fact]
    public void Parse_WithNoArguments_DoesNotEnterInteractiveMode()
    {
        // Arrange
        var args = Array.Empty<string>();

        // Act
        var options = CommandLineInterface.Parse(args);

        // Assert - should return options with defaults, not enter interactive mode
        Assert.NotNull(options);
        Assert.False(options.Interactive);
        Assert.Equal(".", options.Directory); // Default directory
    }

    [Fact]
    public void Parse_WithInteractiveFlag_SetsInteractiveTrue()
    {
        // Arrange
        var args = new[] { "--interactive" };

        // Act
        var options = CommandLineInterface.Parse(args);

        // Assert - Parse should detect interactive flag but not actually run interactive mode
        // (RunInteractive is called after Parse returns)
        Assert.NotNull(options);
        Assert.True(options.Interactive);
    }

    [Fact]
    public void Parse_WithShortInteractiveFlag_SetsInteractiveTrue()
    {
        // Arrange
        var args = new[] { "-i" };

        // Act
        var options = CommandLineInterface.Parse(args);

        // Assert
        Assert.NotNull(options);
        Assert.True(options.Interactive);
    }

    [Fact]
    public void Parse_WithDirectoryArgument_DoesNotEnterInteractiveMode()
    {
        // Arrange
        var args = new[] { "." };

        // Act
        var options = CommandLineInterface.Parse(args);

        // Assert
        Assert.NotNull(options);
        Assert.False(options.Interactive);
        Assert.Equal(".", options.Directory);
    }

    [Fact]
    public void Parse_WithMultipleArguments_DoesNotEnterInteractiveMode()
    {
        // Arrange
        var args = new[] { ".", "--dry-run", "--verbose" };

        // Act
        var options = CommandLineInterface.Parse(args);

        // Assert
        Assert.NotNull(options);
        Assert.False(options.Interactive);
        Assert.True(options.DryRun);
        Assert.True(options.Verbose);
    }

    [Fact]
    public void Parse_WithVariousArguments_NeverEntersInteractiveModeUnlessExplicit()
    {
        // Arrange - test various argument combinations
        var testCases = new[]
        {
            new[] { ".", "-o", "output.md" },
            new[] { ".", "-e", ".cs,.js" },
            new[] { ".", "--dry-run" },
            new[] { ".", "--verbose" },
            new[] { ".", "--exclude", "**/*.log" }
        };

        foreach (var args in testCases)
        {
            // Act
            var options = CommandLineInterface.Parse(args);

            // Assert
            Assert.NotNull(options);
            Assert.False(options.Interactive);
        }
    }
}
