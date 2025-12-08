# Changelog

All notable changes to FileCombiner will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [2.0.0] - 2025-12-08

### Added
- `--interactive` / `-i` flag for opt-in interactive mode
- Comprehensive property-based testing with FsCheck
- Improved help text using CommandLine library's built-in help generation
- Path resolution tests to ensure location independence

### Changed
- **BREAKING**: Interactive mode is now opt-in via `--interactive` flag instead of automatic fallback
- **BREAKING**: Directory argument is now optional (defaults to current directory)
- Simplified interactive mode from 7 questions to 4 essential questions
- Help text now uses CommandLine library's automatic generation
- Fixed Spectre.Console markup escaping in help display
- Fixed service initialization hang caused by circular dependency
- Improved async/await patterns throughout codebase

### Removed
- **BREAKING**: The following options are now hidden (still work for backward compatibility):
  - `--include` - Use `--extensions` instead
  - `--ignore-dirs` - Use `--exclude` with directory patterns instead
  - `--compact` - Feature removed (rarely used)
  - `--no-tree` - Tree generation simplified
  - `--max-files` - Now uses sensible default of 1000
  - `--max-file-size` - Now uses sensible default of 10MB

### Fixed
- Spectre.Console markup parsing errors in help display
- Application hanging after service provider initialization
- Circular dependency between ContentExtractorFactory and FileContentExtractor

## Migration Guide

### For users of removed options:

**`--include` patterns**
- **Before**: `filecombiner . --include "*.cs,*.js"`
- **After**: `filecombiner . --extensions .cs,.js`

**`--ignore-dirs` patterns**
- **Before**: `filecombiner . --ignore-dirs "bin,obj"`
- **After**: `filecombiner . --exclude "**/bin/**,**/obj/**"`

**`--compact` mode**
- This feature has been removed. The default output format is now optimized.

**`--no-tree` flag**
- Tree generation is now always included and optimized.

**`--max-files` and `--max-file-size`**
- These now use sensible defaults (1000 files, 10MB per file).
- If you need different limits, please open an issue.

### For users relying on automatic interactive mode:

**Before**: Running `filecombiner` without arguments would enter interactive mode.

**After**: Running `filecombiner` without arguments processes the current directory with defaults.

To enter interactive mode, use the `--interactive` or `-i` flag:
```bash
filecombiner --interactive
```

### Default behavior changes:

- **No arguments**: Now processes current directory instead of entering interactive mode
- **Directory only**: `filecombiner ./src` now works without additional prompts
- **Sensible defaults**: Auto-detects text files, max depth of 5, outputs to clipboard

## [Previous Versions]

_(No previous versions documented)_
