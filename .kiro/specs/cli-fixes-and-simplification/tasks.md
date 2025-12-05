# Implementation Plan

- [x] 1. Fix Spectre.Console markup escaping in help display





  - Create a `SafeMarkup()` helper method that wraps `Markup.Escape()`
  - Update `ShowHelp()` method to escape all text before display
  - Add fallback to plain `Console.WriteLine()` if markup parsing fails
  - _Requirements: 1.1, 1.2, 1.3, 1.4_

- [x] 1.1 Write property test for text escaping


  - **Property 1: Text escaping prevents markup errors**
  - **Validates: Requirements 1.2**

- [x] 2. Add detailed logging to diagnose service initialization hang



  - Add debug logging before and after each service resolution in `Program.cs`
  - Add debug logging at the start of `DiscoverFilesAsync()`
  - Add timeout detection wrapper for async operations
  - Run the application with verbose logging to identify hang location
  - _Requirements: 5.1, 5.2, 5.3_

- [x] 3. Fix the identified hang issue


  - Based on logging output from task 2, implement the specific fix
  - Verify proper async/await patterns (no `.Result` or `.Wait()`)
  - Ensure all required services are properly registered
  - Test that application proceeds past service initialization
  - _Requirements: 2.1, 2.2, 2.3, 2.4_

- [x] 3.1 Write property test for service resolution


  - **Property 2: Service resolution completes without hanging**
  - **Validates: Requirements 2.2, 2.4**

- [x] 3.2 Write property test for async operations


  - **Property 3: Async operations complete without deadlock**
  - **Validates: Requirements 5.3**

- [x] 4. Simplify CommandLineOptions class



  - Remove or implement unused options: `--include`, `--ignore-dirs`, `--compact`, `--no-tree`, `--max-files`, `--max-file-size`
  - Add `--interactive` / `-i` flag for opt-in interactive mode
  - Update option descriptions and help text
  - _Requirements: 3.1, 3.2, 3.3, 3.4_

- [x] 5. Refactor interactive mode




  - Improve interactive mode questions and UX/UI
  - _Requirements: 6.1, 6.2, 6.3, 6.4_

- [x] 5.1 Write property test for interactive mode


  - **Property 4: Arguments prevent interactive mode**
  - **Validates: Requirements 6.4**

- [x] 6. Update help text to be concise and clear
  - Rewrite `ShowHelp()` to use built in powershell help
  - Add brief tool description at the top
  - Include 2-3 practical usage examples
  - Ensure all text is properly escaped
  - _Requirements: 4.1, 4.2, 4.3, 4.4_

- [x] 7. Verify path handling works after directory moves
  - Test that application resolves paths relative to current working directory
  - Ensure no hardcoded absolute paths exist in the codebase
  - Run application from different directories to verify it works
  - _Requirements: 7.1, 7.3_

- [x] 7.1 Write property test for path resolution independence
  - **Property 5: Path resolution is location-independent**
  - **Validates: Requirements 7.1, 7.3**

- [x] 8. Clean and rebuild project to fix any stale references
  - Run `dotnet clean`
  - Delete `bin` and `obj` directories
  - Run `dotnet restore`
  - Run `dotnet build` to verify successful compilation
  - _Requirements: 7.2, 7.4_

- [x] 9. Create test project structure
  - Create `FileCombiner.Tests` project with xUnit
  - Add FsCheck package for property-based testing
  - Set up test project structure with folders for CLI, Services, and Integration tests
  - Configure test project to reference main project
  - _Requirements: 8.1, 8.2, 8.3_

- [x] 9.1 Write unit tests for help display
  - Test help displays without exceptions
  - Test help contains all expected options
  - Test help with various scenarios
  - _Requirements: 1.1, 1.3_

- [x] 9.2 Write unit tests for option parsing
  - Test each command-line option individually
  - Test option combinations
  - Test invalid option values
  - _Requirements: 3.1, 3.2, 3.3_

- [x] 9.3 Write unit tests for service registration
  - Verify all required services are registered
  - Verify services can be resolved
  - Test service lifetimes
  - _Requirements: 2.4_

- [x] 9.4 Write unit tests for path handling
  - Test absolute path resolution
  - Test relative path resolution
  - Test paths with special characters
  - _Requirements: 7.1, 7.3_

- [x] 9.5 Write integration tests for end-to-end scenarios
  - Test full pipeline: scan → discover → combine → output
  - Test error scenarios: invalid directory, no files, permission denied
  - Test output modes: clipboard, file, dry-run
  - _Requirements: 2.1, 2.3, 3.4_

- [x] 10. Checkpoint - Ensure all tests pass
  - Ensure all tests pass, ask the user if questions arise.

- [x] 11. Create CHANGELOG.md documenting breaking changes
  - Document removed options and their alternatives
  - Document new `--interactive` flag behavior
  - Document simplified default behavior
  - _Requirements: 3.4_

- [x] 12. Update README.md with simplified usage instructions
  - Update command-line options table
  - Update examples to reflect simplified CLI
  - Remove references to removed options
  - Add migration guide for users of removed options
  - _Requirements: 4.1, 4.2, 4.3_

- [x] 13. Final verification and testing
  - Run application with various argument combinations
  - Verify help displays correctly
  - Verify application doesn't hang
  - Verify all core use cases work
  - Run full test suite and verify all tests pass
  - _Requirements: All_
