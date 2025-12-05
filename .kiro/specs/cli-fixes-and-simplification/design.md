# Design Document

## Overview

This design addresses critical bugs and complexity issues in the FileCombiner CLI application. The solution involves three main components: (1) fixing Spectre.Console markup escaping in help text, (2) diagnosing and resolving the service initialization hang, and (3) simplifying the CLI interface by reducing options and removing unnecessary interactive mode complexity.

The root causes identified are:
- Unescaped text in help display containing reserved Spectre.Console markup keywords
- Potential async/await deadlock or missing service registration causing the hang after DI container build
- Over-engineered CLI with too many options that confuse users

## Architecture

The application follows a layered architecture:

```
Program.cs (Entry Point)
    ↓
CommandLineInterface (CLI Parsing & Help)
    ↓
AppConfig (Configuration)
    ↓
Service Provider (DI Container)
    ↓
Services (Discovery, Combiner, etc.)
```

### Key Changes

1. **Help Display**: Refactor `ShowHelp()` to properly escape all text before passing to Spectre.Console
2. **Service Resolution**: Add detailed logging and async debugging to identify hang location
3. **CLI Simplification**: Reduce command-line options from 13 to 8 core options
4. **Interactive Mode**: Make interactive mode opt-in via `--interactive` flag instead of default

## Components and Interfaces

### CommandLineInterface (Modified)

**Responsibilities:**
- Parse command-line arguments
- Display help text safely with proper escaping
- Handle simplified option set

**Key Methods:**
```csharp
public static CommandLineOptions? Parse(string[] args)
public static void ShowHelp() // Fixed with proper escaping
public static void PrintSummary(ProcessResult result)
public static Task OutputResult(CombinedFilesData data, AppConfig config)
```

**Changes:**
- Wrap all help text strings in `Markup.Escape()` before display
- Remove automatic interactive mode fallback
- Simplify help text to show only essential options

### CommandLineOptions (Simplified)

**Core Options to Keep:**
1. `directory` - Source directory (positional, required)
2. `--output` / `-o` - Output file path
3. `--extensions` / `-e` - File extensions filter
4. `--max-depth` / `-r` - Maximum recursion depth
5. `--exclude` - Exclude patterns
6. `--dry-run` - Preview mode
7. `--verbose` / `-v` - Verbose logging
8. `--interactive` / `-i` - Enter interactive mode

**Options to Remove:**
- `--include` (redundant with extensions)
- `--ignore-dirs` (use exclude patterns)
- `--compact` (rarely used)
- `--no-tree` (tree should be optional via flag instead)
- `--max-files` (use sensible default)
- `--max-file-size` (use sensible default)

### Program.cs (Debugging Enhanced)

**Changes:**
- Add detailed logging before and after each service resolution
- Add timeout detection for async operations
- Ensure proper async/await patterns (no `.Result` or `.Wait()`)
- Add try-catch around service resolution with specific error messages

## Data Models

No changes to existing data models (`DiscoveredFile`, `ProcessResult`, `CombinedFilesData`). The models are well-designed and not contributing to the issues.

## Correctness Properties


*A property is a characteristic or behavior that should hold true across all valid executions of a system-essentially, a formal statement about what the system should do. Properties serve as the bridge between human-readable specifications and machine-verifiable correctness guarantees.*

### Property Reflection

After reviewing the prework analysis, I identified several areas where properties can be consolidated:

- Requirements 1.2 and 5.3 both relate to handling text safely without errors - these can be combined into a general robustness property
- Requirements 2.2 and 2.4 both test service resolution behavior - these can be combined
- Requirements 6.4 and 7.1 both test path handling across different contexts - these share similar testing approaches

The following properties represent the unique, non-redundant correctness guarantees:

Property 1: Text escaping prevents markup errors
*For any* string containing potential Spectre.Console markup keywords, when that string is escaped and displayed, the system should not throw markup parsing exceptions
**Validates: Requirements 1.2**

Property 2: Service resolution completes without hanging
*For any* required service type in the DI container, when that service is resolved, the operation should complete within a reasonable timeout (5 seconds)
**Validates: Requirements 2.2, 2.4**

Property 3: Async operations complete without deadlock
*For any* async operation in the application flow, when that operation is awaited, it should complete within a reasonable timeout without deadlocking
**Validates: Requirements 5.3**

Property 4: Arguments prevent interactive mode
*For any* non-empty command-line argument list, when the application is started with those arguments, interactive mode should not be entered
**Validates: Requirements 6.4**

Property 5: Path resolution is location-independent
*For any* working directory, when the application resolves file paths, all paths should be relative to the current working directory and not contain hardcoded absolute paths from other locations
**Validates: Requirements 7.1, 7.3**

## Error Handling

### Markup Parsing Errors

**Strategy**: Defensive escaping at the boundary
- All user-provided text and file paths must be escaped before passing to Spectre.Console
- Implement a helper method `SafeMarkup(string text)` that wraps `Markup.Escape()`
- Use `SafeMarkup()` consistently throughout the codebase

**Error Recovery**:
- If markup parsing fails despite escaping, catch the exception and fall back to plain `Console.WriteLine()`
- Log the error for debugging but don't crash the application

### Service Resolution Failures

**Strategy**: Fail-fast with clear error messages
- Wrap service resolution in try-catch blocks
- Provide specific error messages indicating which service failed to resolve
- Include instructions for common fixes (e.g., "Run 'dotnet restore'")

**Error Recovery**:
- No recovery possible if core services fail to resolve
- Exit with error code 1 and clear error message

### Async Deadlocks

**Strategy**: Proper async/await patterns
- Never use `.Result` or `.Wait()` on async operations
- Always use `await` with `ConfigureAwait(false)` for library code
- Use `async Task Main` pattern

**Detection**:
- Add timeout wrappers around critical async operations
- If timeout is exceeded, log detailed diagnostic information
- Exit with error indicating potential deadlock

### File System Errors

**Strategy**: Graceful degradation
- Catch `UnauthorizedAccessException`, `IOException`, `DirectoryNotFoundException`
- Log the error with the specific file/directory path
- Continue processing other files when possible
- Only fail completely if the root directory is inaccessible

## Testing Strategy

### Unit Testing Framework

We will use **xUnit** as the testing framework for .NET, which is the industry standard and integrates well with Visual Studio and Rider.

**Test Organization**:
```
FileCombiner.Tests/
  ├── CLI/
  │   ├── CommandLineInterfaceTests.cs
  │   └── CommandLineOptionsTests.cs
  ├── Services/
  │   ├── FileDiscoveryServiceTests.cs
  │   └── FileCombinerServiceTests.cs
  └── Integration/
      └── EndToEndTests.cs
```

### Unit Tests

Unit tests will cover:

1. **Help Display Tests**
   - Verify help text contains all expected options
   - Verify help displays without exceptions
   - Test with various terminal widths

2. **Option Parsing Tests**
   - Test each command-line option individually
   - Test option combinations
   - Test invalid option values
   - Test missing required options

3. **Service Registration Tests**
   - Verify all required services are registered
   - Verify services can be resolved
   - Test service lifetimes (singleton, transient, etc.)

4. **Path Handling Tests**
   - Test absolute path resolution
   - Test relative path resolution
   - Test paths with special characters
   - Test cross-platform path separators

5. **Interactive Mode Tests**
   - Verify interactive mode is not entered with arguments
   - Verify interactive mode prompts appear with `--interactive` flag
   - Test default value handling in interactive mode

### Property-Based Testing

We will use **FsCheck** for property-based testing in C#. FsCheck is a mature library that integrates well with xUnit.

**Configuration**:
- Each property test should run a minimum of 100 iterations
- Use custom generators for domain-specific types (file paths, extensions, etc.)

**Property Tests**:

Each property test will be tagged with a comment referencing the design document property it implements, using this format: `// Feature: cli-fixes-and-simplification, Property {number}: {property_text}`

1. **Property Test 1: Text Escaping**
   ```csharp
   // Feature: cli-fixes-and-simplification, Property 1: Text escaping prevents markup errors
   [Property(MaxTest = 100)]
   public Property EscapedTextNeverThrowsMarkupException(string arbitraryText)
   ```
   - Generate random strings including Spectre.Console keywords
   - Verify escaped text doesn't throw exceptions when displayed

2. **Property Test 2: Service Resolution Timeout**
   ```csharp
   // Feature: cli-fixes-and-simplification, Property 2: Service resolution completes without hanging
   [Property(MaxTest = 100)]
   public Property ServiceResolutionCompletesWithinTimeout(Type serviceType)
   ```
   - Generate service types from registered services
   - Verify each resolves within 5 seconds

3. **Property Test 3: Async Completion**
   ```csharp
   // Feature: cli-fixes-and-simplification, Property 3: Async operations complete without deadlock
   [Property(MaxTest = 100)]
   public Property AsyncOperationsCompleteWithoutDeadlock(string directory)
   ```
   - Generate random valid directory paths
   - Verify file discovery completes within timeout

4. **Property Test 4: Arguments Prevent Interactive Mode**
   ```csharp
   // Feature: cli-fixes-and-simplification, Property 4: Arguments prevent interactive mode
   [Property(MaxTest = 100)]
   public Property ArgumentsPreventInteractiveMode(NonEmptyArray<string> args)
   ```
   - Generate random non-empty argument lists
   - Verify interactive mode is never entered

5. **Property Test 5: Path Resolution Independence**
   ```csharp
   // Feature: cli-fixes-and-simplification, Property 5: Path resolution is location-independent
   [Property(MaxTest = 100)]
   public Property PathResolutionIsLocationIndependent(string workingDirectory)
   ```
   - Generate random working directories
   - Verify resolved paths are always relative to current directory

### Integration Tests

Integration tests will verify end-to-end scenarios:

1. **Full Pipeline Test**: Directory scan → file discovery → combining → output
2. **Error Scenario Tests**: Invalid directory, no files found, permission denied
3. **Output Mode Tests**: Clipboard output, file output, dry-run mode

### Test Execution

- Tests should run on every build
- CI/CD pipeline should fail if any test fails
- Code coverage reports should be generated
- Target: 80% code coverage for core functionality

## Implementation Notes

### Debugging the Hang Issue

To identify the hang location, we'll add detailed logging:

```csharp
_logger.LogDebug("About to resolve IFileDiscoveryService");
var discoveryService = provider.GetRequiredService<IFileDiscoveryService>();
_logger.LogDebug("Successfully resolved IFileDiscoveryService");

_logger.LogDebug("About to resolve IFileCombinerService");
var combinerService = provider.GetRequiredService<IFileCombinerService>();
_logger.LogDebug("Successfully resolved IFileCombinerService");

_logger.LogDebug("About to call DiscoverFilesAsync");
var discoveryResult = await discoveryService.DiscoverFilesAsync(config);
_logger.LogDebug("Successfully completed DiscoverFilesAsync");
```

### Simplified Help Text

The new help text should be concise and clear:

```
FileCombiner - Combine multiple files into a single reference document

Usage: filecombiner <directory> [options]

Options:
  -o, --output <file>       Output file path (default: clipboard)
  -e, --extensions <list>   File extensions (comma-separated, default: auto-detect)
  -r, --max-depth <n>       Maximum directory depth (default: 5)
  --exclude <patterns>      Exclude files/dirs matching patterns
  --dry-run                 Preview without processing
  -v, --verbose             Enable verbose output
  -i, --interactive         Enter interactive mode
  -h, --help                Show this help

Examples:
  filecombiner .
  filecombiner ./src -e .cs,.js -o combined.md
  filecombiner ./project --exclude "**/bin/**,**/obj/**"
```

### Migration Path

For users relying on removed options:

- `--include`: Use `--extensions` instead
- `--ignore-dirs`: Use `--exclude` with directory patterns
- `--compact`: Remove this feature (rarely used)
- `--no-tree`: Tree generation will be simplified and always included
- `--max-files`: Use sensible default of 1000
- `--max-file-size`: Use sensible default of 10MB

Document these changes in a CHANGELOG.md file.
