# FileCombiner

[![CI](https://github.com/Kydoimos97/FileCombiner/actions/workflows/ci.yml/badge.svg)](https://github.com/Kydoimos97/FileCombiner/actions/workflows/ci.yml)
[![codecov](https://codecov.io/gh/Kydoimos97/FileCombiner/branch/main/graph/badge.svg)](https://codecov.io/gh/Kydoimos97/FileCombiner)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4)](https://dotnet.microsoft.com/download)
[![Release](https://img.shields.io/github/v/release/Kydoimos97/FileCombiner)](https://github.com/Kydoimos97/FileCombiner/releases/latest)

A high-performance .NET CLI tool that combines multiple files from a directory structure into a single reference document. Perfect for code reviews, documentation, or sharing project snapshots with AI assistants.

> **Version 2.1** - Now with simplified CLI, opt-in interactive mode, and comprehensive test coverage (83%+)

## Features

- **Smart File Discovery**: Recursively scans directories with configurable depth limits
- **Text Auto-Detection**: Automatically identifies text files vs binary files when using wildcard mode
- **Rich Filtering**: Support for glob patterns, file size limits, and extension filtering
- **Language Detection**: Automatic syntax highlighting based on file extensions
- **Directory Tree Generation**: Visual directory structure in output
- **Multiple Output Modes**: Copy to clipboard (default) or save to file
- **Opt-in Interactive Mode**: Simplified 4-question workflow with 60-second timeout
- **Dry Run Support**: Preview what will be processed before execution
- **Built-in Safeguards**: Ignores common build directories, caches, and system files
- **Comprehensive Testing**: 83%+ code coverage with property-based testing

## What's New in v2.1

### v2.1.0 (Latest)
- **Fixed**: Interactive mode now times out after 60 seconds to prevent deployment locks

### v2.0.0 (Major Release)
- **Breaking**: Interactive mode is now opt-in via `-i` flag (no longer automatic)
- **Breaking**: Directory argument is optional (defaults to current directory)
- **Improved**: Simplified interactive mode from 7 questions to 4 essential questions
- **Improved**: Better help text and error messages
- **Fixed**: Service initialization hangs and circular dependencies
- **Fixed**: Spectre.Console markup escaping issues
- **Added**: Comprehensive test coverage (83%+) with property-based testing
- **Added**: Multi-platform releases (Windows, Linux, macOS - x64 and ARM64)

See [CHANGELOG.md](CHANGELOG.md) for complete version history and migration guide.

## Installation

### Download Releases

Pre-built binaries are available from the [releases page](https://github.com/Kydoimos97/FileCombiner/releases/latest) for:
- Windows (x64, ARM64)
- Linux (x64, ARM64) 
- macOS (x64, ARM64)

### Build from Source

```bash
git clone https://github.com/Kydoimos97/FileCombiner.git
cd FileCombiner
dotnet publish -c Release -r win-x64 --self-contained
```

## Usage

### Quick Start

```bash
# Combine all text files in current directory (auto-detects text files)
filecombiner

# Combine files from a specific directory
filecombiner ./src

# Combine specific file types
filecombiner ./src -e .cs,.js,.ts

# Save to file instead of clipboard
filecombiner ./project -o combined-output.md

# Preview what would be processed (dry run)
filecombiner ./src --dry-run

# Interactive mode with prompts (times out after 60 seconds)
filecombiner -i
```

### What's New in v2.0+

**Breaking Changes:**
- Interactive mode is now **opt-in** via `-i` flag (was automatic fallback in v1)
- Directory argument is now **optional** (defaults to current directory)
- Simplified from 7 questions to 4 essential questions in interactive mode

**Improvements:**
- Better help text using CommandLine library
- Fixed Spectre.Console markup escaping issues
- Resolved service initialization hangs
- Interactive mode auto-exits after 60 seconds to prevent deployment locks
- Comprehensive test coverage (83%+) with property-based testing

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

### Hidden Options (Backward Compatibility)

The following options are hidden from help but still work for backward compatibility:

- `--include` - Use `--extensions` instead
- `--ignore-dirs` - Use `--exclude` with directory patterns instead
- `--compact` - Feature removed (rarely used)
- `--no-tree` - Tree generation is now always included
- `--max-files` - Now uses sensible default of 1000
- `--max-file-size` - Now uses sensible default of 10MB

**Migration from v1.x:**
```bash
# Old v1.x syntax
filecombiner . --include "*.cs,*.js" --ignore-dirs "bin,obj"

# New v2.x syntax
filecombiner . --extensions .cs,.js --exclude "**/bin/**,**/obj/**"
```

See [CHANGELOG.md](CHANGELOG.md) for complete migration guide.

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

# Run tests with coverage (using Coverlet)
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura

# Run specific test category
dotnet test --filter "FullyQualifiedName~PathResolutionTests"
```

### Test Coverage

- **Unit Tests**: Core functionality and edge cases
- **Property-Based Tests**: Correctness guarantees with FsCheck (100+ iterations per property)
- **Integration Tests**: End-to-end scenarios
- **Binary File Tests**: Real-world file format testing (CSV, XLSX, PPTX, PDF, DOC, RTF)

**Current Coverage**: 83.42% line coverage (exceeds 80% target)
- 27+ test classes covering all critical paths
- Untestable code (Program.cs entry point, interactive console input) excluded from coverage

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
