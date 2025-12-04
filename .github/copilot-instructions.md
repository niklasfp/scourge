# Copilot Instructions for Scourge

## Project Overview

**Scourge** is a .NET library designed to inflict controlled "pain" on .NET applications for testing and stress purposes. It provides both a core library and ASP.NET Core integration for testing application resilience under adverse conditions like memory pressure, CPU load, exceptions, and other controlled failure scenarios.

## Technology Stack

- **Framework**: .NET 10.0 (`net10.0`)
- **Language**: C# 14 with nullable reference types enabled
- **Test Framework**: xUnit with Shouldly assertions
- **Build System**: .NET SDK 10.0.x with MSBuild
- **Project Structure**: Multi-project solution with central package management

## Project Structure

```
src/
├── Scourge/                      # Core library for inflicting "pain"
│   ├── Diagnostics/              # System diagnostics and metrics
│   ├── Hurt/                     # CPU/thread stress utilities
│   └── Memory/                   # Memory allocation utilities
├── Scourge.AspNetCore/          # ASP.NET Core integration (REST APIs)
│   ├── GarbageCollector/        # GC control endpoints
│   ├── Hurt/                    # Hurt endpoints
│   ├── Information/             # System info endpoints
│   ├── Logging/                 # Logging endpoints
│   └── Memory/                  # Memory endpoints
├── Scourge.Tests/               # Unit tests for core library
└── Contallocator/               # Sample/test ASP.NET Core application
```

## Build and Test Commands

All commands should be run from the `src/` directory:

```bash
# Restore dependencies
dotnet restore

# Build the solution
dotnet build --no-restore -c Release

# Run tests
dotnet test --no-build --verbosity normal -c Release

# Build a specific project
dotnet build src/Scourge/Scourge.csproj --no-restore -c Release
```

## Code Style and Conventions

### General Guidelines

- **Warnings as Errors**: Enabled via `TreatWarningsAsErrors` - all code must compile without warnings
- **Nullable Reference Types**: Enabled - always handle nullability explicitly
- **Implicit Usings**: Enabled - common namespaces are available automatically
- **Documentation**: XML documentation comments are preferred for public APIs
- **ReSharper Directives**: Used in some files to suppress specific inspections

### Naming Conventions

- **Classes/Interfaces**: PascalCase (e.g., `ThreadWhipper`, `IWorkManager`)
- **Methods**: PascalCase (e.g., `DoNothingWork`, `AllocateManaged`)
- **Properties**: PascalCase (e.g., `ProcessorCount`, `IsRunning`)
- **Private Fields**: _camelCase with underscore prefix (e.g., `_threads`, `_lockToken`)
- **Parameters**: camelCase (e.g., `threadCount`, `cancellationToken`)

### Code Patterns

#### Testing with xUnit and Shouldly

```csharp
[Fact]
public void ShouldReturnSameProcessorCountAsEnvironment()
{
    ThreadWhipper.ProcessorCount.ShouldBe(Environment.ProcessorCount);
}

[Theory]
[InlineData(1)]
[InlineData(2)]
public void ShouldBeAbleToStartAndStopThreads(int threadCount)
{
    using var cts = new CancellationTokenSource();
    var work = ThreadWhipper.DoNothingWork(threadCount, cts.Token);
    work.ThreadCount.ShouldBe(threadCount);
    work.IsRunning.ShouldBeTrue();
}
```

#### Extension Methods

Extension methods are placed in the appropriate Microsoft namespace to provide seamless integration:

```csharp
// In namespace Microsoft.Extensions.DependencyInjection
public static IServiceCollection AddScourge(this IServiceCollection services)

// In namespace Microsoft.AspNetCore.Builder
public static IEndpointRouteBuilder MapScourge(this IEndpointRouteBuilder endpoints, string prefix)
```

#### Thread-Safe Patterns

Use lock objects for thread synchronization:

```csharp
private readonly Lock _lockToken = new();

public bool IsRunning
{
    get
    {
        lock (_lockToken)
        {
            return _isRunning;
        }
    }
}
```

## Boundaries and Constraints

### What Should Be Modified

- Core library functionality in `src/Scourge/`
- ASP.NET Core endpoints in `src/Scourge.AspNetCore/`
- Unit tests in `src/Scourge.Tests/`
- Sample application in `src/Contallocator/` (for demonstration purposes)
- Documentation files (README.md, docs/)

### What Should NOT Be Modified

- `.git/` directory
- `bin/` and `obj/` directories (build outputs)
- `.github/workflows/` - CI/CD configuration (unless explicitly requested)
- `Directory.Build.props` - Central build configuration (unless explicitly needed)
- `Directory.Packages.props` - Central package management (unless explicitly needed)
- `global.json` - SDK version pinning (unless explicitly needed)
- Existing test assertions unless fixing a bug

### Security Considerations

- Never commit secrets, API keys, or credentials
- Be cautious with unsafe code blocks (already used for unmanaged memory)
- Validate all user inputs in API endpoints
- Document any intentionally dangerous features (this is a "hurt" library after all)

## Common Tasks

### Adding a New "Hurt" Feature

1. Add the core functionality in `src/Scourge/Hurt/`
2. Add corresponding tests in `src/Scourge.Tests/`
3. Add API endpoints in `src/Scourge.AspNetCore/Hurt/`
4. Register endpoints in `ApplicationBuilderExtensions.cs`
5. Update README.md with examples

### Adding a New API Endpoint

1. Create endpoint definitions using Minimal API pattern
2. Use `MapGroup()` for logical grouping
3. Follow the existing pattern in `ApplicationBuilderExtensions.MapScourge()`
4. Include OpenAPI/Swagger annotations

### Adding Tests

1. Use xUnit `[Fact]` for single test cases
2. Use xUnit `[Theory]` with `[InlineData]` for parameterized tests
3. Use Shouldly assertions (e.g., `.ShouldBe()`, `.ShouldBeTrue()`)
4. Follow naming: `Should{ExpectedBehavior}When{Condition}` or `Should{ExpectedBehavior}`
5. Clean up resources with `using` statements or try-finally blocks

## Dependencies

The solution uses central package management via `Directory.Packages.props`. Common packages include:

- **xUnit**: Test framework
- **Shouldly**: Assertion library
- **coverlet.collector**: Code coverage
- **ASP.NET Core**: For web API integration (implicitly via SDK)

## Documentation Paths Ignored by CI

The CI workflow ignores changes to:
- `**/*.md` files
- `docs/**` directory

This means documentation-only changes won't trigger builds.

## Special Notes

- This is a "hurt" library - it's designed to cause controlled problems for testing purposes
- Unsafe code blocks are intentionally used for unmanaged memory allocation
- Some features like `StackOverflowException` are deliberately hard to handle
- The library targets containerized environments primarily
- Security is important, but this is a development/testing tool, not production code

## Getting Help

- Review the main [README.md](../README.md) for usage examples
- Check existing tests for patterns
- Look at the `Contallocator` sample project for integration examples
- The project is still work in progress - some features may be incomplete
