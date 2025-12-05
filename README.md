# FileCombiner

[![CI](https://github.com/Kydoimos97/FileCombiner/actions/workflows/ci.yml/badge.svg)](https://github.com/Kydoimos97/FileCombiner/actions/workflows/ci.yml)
[![codecov](https://codecov.io/gh/Kydoimos97/FileCombiner/branch/main/graph/badge.svg)](https://codecov.io/gh/Kydoimos97/FileCombiner)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4)](https://dotnet.microsoft.com/download)

A high-performance .NET CLI tool that combines multiple files from a directory structure into a single reference document. Perfect for code reviews, documentation, or sharing project snapshots with AI assistants.

## Features

- **Smart File Discovery**: Recursively scans directories with configurable depth limits
- **Text Auto-Detection**: Automatically identifies text files vs binary files when using wildcard mode
- **Rich Filtering**: Support for include/exclude patterns, file size limits, and extension filtering
- **Language Detection**: Automatic syntax highlighting based on file extensions
- **Directory Tree Generation**: Optional visual directory structure in output
- **Multiple Output Modes**: Copy to clipboard (default) or save to file
- **Compact Mode**: Remove comments and excessive whitespace for cleaner output
- **Dry Run Support**: Preview what will be processed before execution
- **Built-in Safeguards**: Ignores common build directories, caches, and system files

## Installation

### Download Releases

Pre-built binaries are available from the [releases page](../../releases) for:
- Windows (x64)
- Linux (x64) 
- macOS (x64/ARM64)

### Build from Source

```bash
git clone https://github.com/Kydoimos97/FileCombiner.git
cd FileCombiner
dotnet publish -c Release -r win-x64 --self-contained
```

## Usage

### Basic Usage

```bash
# Combine all text files in current directory (uses sensible defaults)
filecombiner

# Combine files from a specific directory
filecombiner ./src

# Combine specific file types
filecombiner ./src -e .cs,.js,.ts

# Save to file instead of clipboard
filecombiner ./project -o combined-output.md

# Preview what would be processed (dry run)
filecombiner ./src --dry-run

# Interactive mode (prompts for configuration)
filecombiner --interactive
```

### Advanced Options

```bash
# Exclude patterns with glob support
filecombiner ./src --exclude "**/bin/**,**/obj/**,**/node_modules/**"

# Limit directory depth
filecombiner ./project --max-depth 3

# Verbose output for debugging
filecombiner ./src -v
```

## Command Line Options

| Option | Short | Description | Default |
|--------|-------|-------------|---------|
| `directory` | | Source directory to scan | `.` (current directory) |
| `--output` | `-o` | Output file path | Copy to clipboard |
| `--extensions` | `-e` | File extensions (comma-separated, `*` for auto-detect) | `*` (auto-detect) |
| `--max-depth` | `-r` | Maximum directory depth | `5` |
| `--exclude` | | Exclude files/dirs matching glob patterns | |
| `--dry-run` | | Show what would be processed | `false` |
| `--verbose` | `-v` | Enable detailed logging | `false` |
| `--interactive` | `-i` | Enter interactive mode with prompts | `false` |
| `--help` | `-h` | Show help message | |

### Deprecated Options (Still Supported)

The following options are deprecated but still work for backward compatibility:

- `--include` - Use `--extensions` instead
- `--ignore-dirs` - Use `--exclude` with directory patterns instead
- `--compact` - Feature removed
- `--no-tree` - Tree generation is now always included
- `--max-files` - Now uses default of 1000
- `--max-file-size` - Now uses default of 10MB

See [CHANGELOG.md](CHANGELOG.md) for migration guide.

## Examples

### Code Review Preparation
```bash
# Combine all source files for review
filecombiner ./src -e .cs,.js,.ts -o review.md
```

### Documentation Generation
```bash
# Include only documentation files
filecombiner ./docs -e .md,.rst --max-depth 2
```

### Project Snapshot
```bash
# Create complete project snapshot excluding build artifacts
filecombiner . --exclude "**/bin/**,**/obj/**,**/node_modules/**"
```

### Large Codebase Analysis
```bash
# Process large codebase with verbose output
filecombiner ./enterprise-app -v
```

## Output Format

The tool generates a structured markdown document containing:

1. **Header**: Title and description
2. **Directory Structure**: Visual tree of included directories (optional)
3. **File Contents**: Each file with:
   - Relative path as header
   - Syntax-highlighted code block
   - Language detection based on file extension

### Sample Output
```markdown
## Combined Files Reference

This is a combined file representative of a folder structure, only used as reference.

## Directory Structure

- `src`

-------------------------------------------------------------------------------

### `src\Program.cs`
```csharp
using System;

namespace MyApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
        }
    }
}
```

### Sample CLI
<img width="425" height="342" alt="image" src="https://github.com/user-attachments/assets/18f780b5-41b0-4460-b430-a5dd9b7168a3" />


## Default Exclusions

The tool automatically ignores common build and cache directories:

- **Version Control**: `.git`, `.svn`, `.hg`, `.bzr`, `_darcs`
- **Build Artifacts**: `bin`, `obj`, `build`, `dist`, `target`
- **Dependencies**: `node_modules`, `.venv`, `venv`, `env`
- **IDE Files**: `.vs`, `.vscode`, `.idea`
- **Cache/Temp**: `__pycache__`, `.pytest_cache`, `.mypy_cache`, `.tox`
- **Coverage**: `coverage`, `.coverage`, `htmlcov`, `.nyc_output`
- **System Files**: `.DS_Store`, `Thumbs.db`, `desktop.ini`

## Performance

- **Memory Efficient**: Processes files individually, not loading entire directory into memory
- **Fast Scanning**: Optimized directory traversal with early termination
- **Size Limits**: Built-in protection against processing huge files

## Testing

The project includes comprehensive test coverage with xUnit and FsCheck for property-based testing.

```bash
# Run all tests
dotnet test

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test category
dotnet test --filter "FullyQualifiedName~PathResolutionTests"
```

### Test Coverage

- **Unit Tests**: Core functionality and edge cases
- **Property-Based Tests**: Correctness guarantees with FsCheck (100+ iterations per property)
- **Integration Tests**: End-to-end scenarios

Current test coverage: 27 tests covering all critical paths.

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Follow existing code patterns and SOLID principles
4. Add tests for new functionality (required for PR approval)
5. Ensure all tests pass (`dotnet test`)
6. Commit your changes (`git commit -m 'Add amazing feature'`)
7. Push to the branch (`git push origin feature/amazing-feature`)
8. Open a Pull Request

### Code Quality Requirements

- All tests must pass
- Code coverage should not decrease
- Follow C# coding conventions
- Add XML documentation for public APIs

## License

MIT License
