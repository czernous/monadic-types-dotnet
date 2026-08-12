# Development Policy

## Toolchain

`global.json` requires the .NET 10.0.300 feature band and rolls forward only to
the latest installed patch in that band. C# 14 is explicit rather than inherited
from whichever newer SDK happens to be installed.

Restore, compile, and test from the repository root:

```powershell
dotnet restore MonadicTypes.slnx
dotnet build MonadicTypes.slnx -c Release --no-restore
dotnet test MonadicTypes.slnx -c Release --no-build --no-restore
```

Verify the NativeAOT integration boundary on Windows:

```powershell
dotnet publish tests\MonadicTypes.AotSmoke\MonadicTypes.AotSmoke.csproj `
  -c Release -r win-x64 --no-restore
```

The same smoke project should be published with the appropriate runtime
identifier on Linux and macOS once those environments are available.

## Formatting

EditorConfig is authoritative for Rider, Visual Studio, the .NET CLI, and other
compatible editors. Files use LF endings on every platform; `.gitattributes`
prevents local Git settings from rewriting them.

Apply and verify deterministic whitespace formatting with:

```powershell
dotnet format whitespace MonadicTypes.slnx --no-restore
dotnet format whitespace MonadicTypes.slnx --verify-no-changes --no-restore
```

Semantic and style diagnostics are enforced by `dotnet build`. A complete
`dotnet format` analyzer pass is not the gate because design-time formatting does
not execute the source-generator attribute injection used by consumer tests.

## Analysis Layers

Every project enables:

- nullable reference analysis;
- the .NET SDK's pinned `10-recommended` analyzer profile;
- build-time IDE style diagnostics;
- MIT-licensed Meziantou correctness, security, and usage diagnostics;
- deterministic builds and NuGet vulnerability auditing; and
- warnings as errors.

Shipping projects under `src` additionally use Microsoft's MIT-licensed banned
API analyzer. `BannedSymbols.txt` rejects reflection, runtime assembly loading,
dynamic dispatch, expression-tree compilation, and runtime code generation.
Tests may use those facilities when they are necessary to construct a test
harness; they do not enter runtime packages.

Analyzer packages are private build assets. They do not flow to consumers or
increase runtime or NativeAOT binary size. SonarAnalyzer.CSharp is intentionally
not referenced because its current source-available license does not fit this
repository's commercial and AI-assisted use requirements.

## Rule Exceptions

Exceptions belong in the narrowest applicable EditorConfig path and require a
comment explaining the semantic, test, generator, or benchmark constraint. The
current exceptions preserve:

- conventional C# FP names such as `Option`, `Error`, and `Result<T, E>`;
- static generic construction APIs such as `Option<T>.Some` and
  `Result<T, E>.Ok`;
- mutable callable-struct behavior without defensive copies;
- underscore-separated behavioral test names;
- source-generator test metadata construction; and
- synchronous consumption of known-completed `ValueTask` instances in isolated
  benchmarks.

Do not suppress a diagnostic repository-wide merely to make a build pass. Fix
the finding unless the rule conflicts with a documented public contract,
correctness property, generated-code boundary, or measured hot-path requirement.
