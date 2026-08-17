# Development Policy

## Toolchain

`global.json` requires the .NET 10.0.300 feature band and rolls forward only to
the latest installed patch in that band. C# 14 is explicit rather than inherited
from whichever newer SDK happens to be installed.

Restore, compile, and test from the repository root:

```powershell
eng\tools\win-x64\mt-verify-locks.exe
dotnet restore MonadicTypes.slnx
dotnet build MonadicTypes.slnx -c Release --no-restore
dotnet test MonadicTypes.slnx -c Release --no-build --no-restore
```

Verify the NativeAOT integration boundary on Windows:

```powershell
dotnet restore tests\MonadicTypes.AotSmoke\MonadicTypes.AotSmoke.csproj `
  -r win-x64 --force-evaluate `
  -p:RestorePackagesWithLockFile=false `
  -p:RestoreLockedMode=false `
  -p:NuGetLockFilePath="$env:TEMP\monadic-types-win-x64.lock.json"
dotnet publish tests\MonadicTypes.AotSmoke\MonadicTypes.AotSmoke.csproj `
  -c Release -r win-x64 --no-restore
```

Explicit RID restores disable lock-file writing with global MSBuild properties,
so every project in the restore graph receives the policy and cannot add
host-specific targets to committed portable locks. The precompiled
`mt-verify-locks` NativeAOT tool parses every shipping lock before CI and release
restores. Linux uses the matching executable under `eng/tools/linux-x64`. Use
the appropriate runtime identifier and temporary path for AOT restores.

Ordinary development and CI execute checked-in NativeAOT tools without SDK
startup or script compilation. A tooling-source change incrementally rebuilds
only its affected command on `win-x64` and `linux-x64`; changing
`eng/NativeTool.props` rebuilds all four commands for both supported tooling
hosts. NativeAOT requires each binary to be linked on its target operating
system, so Windows and Docker produce the committed Windows and Linux artifacts.
The source remains portable, but macOS tooling binaries and CI validation are
deferred until the project has a macOS maintainer.

NativeAOT output is not byte-reproducible across build hosts. CI therefore does
not compare locally produced executables byte for byte. It compiles every
affected command from source for Windows and Linux, runs the tooling unit tests,
and exercises the committed commands in affected-project, lock, package, and
package-consumption jobs. Regenerate the committed executable whenever its
source changes; generated binary changes intentionally trigger the same source
compilation and behavioral gates.

The four commands have deliberately separate binaries:

- `mt-affected` consumes NUL-delimited Git paths, resolves the reverse project
  graph, and writes optional CI outputs without materializing changed paths as
  managed strings.
- `mt-verify-locks` validates shipping lockfiles discovered from the solution
  with pooled UTF-8 parsing and a bounded JSON reader.
- `mt-pack` performs one parallel MSBuild traversal and validates package
  contents with span-based SemVer and Nuspec parsing.
- `mt-test-packages` uses one MSBuild host for parallel default NativeAOT and
  XML-comment opt-in configurations. It requires a unique SemVer that is absent
  from the NuGet global package cache, preventing stale package validation
  without mutating shared cache state or discarding third-party dependencies.

Changes to `eng/Pack.proj` or `eng/TestPackages.proj` rerun package validation
but do not relink an unchanged native executable. See
[tooling performance](tooling-performance.md) for the accepted measurements and
comparison limits.

Build one command for the current host with:

```powershell
dotnet publish eng\MonadicTypes.AffectedProjects.Tool\MonadicTypes.AffectedProjects.Tool.csproj `
  -c Release -r win-x64 -o eng\tools\win-x64
```

The same project accepts `linux-x64` when run on Linux. Replace the project path
to rebuild `mt-pack`, `mt-test-packages`, or `mt-verify-locks`.

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
