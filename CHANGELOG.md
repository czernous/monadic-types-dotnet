# Changelog

Notable user-visible changes are recorded here. The project follows Semantic
Versioning as described in [the release policy](docs/releases.md).

## Unreleased

No unreleased changes.

## [0.2.0-preview.1] - 2026-08-17

### Added

- `MonadicTypes.NET.Collections` with first-failure traversal for spans and
  count-known lists, including delegate, caller-state, and struct-callable
  selectors.
- `MonadicTypes.NET.Linq` with opt-in `Select`, `SelectMany`, and Option `Where`
  extension members for fluent and query composition.
- Result combination and mapping overloads for two through six independent
  results.
- Result and Option positional deconstruction for allocation-free pattern
  matching.
- Reference- and value-nullable Option factories and boundary conversions.
- Fallible Option traversal, Result/Option transpose, flattening, and
  `RequireSome` composition.
- `MonadicTypes.NET.AspNetCore.OpenApi` with reflection-free error catalogs,
  status-scoped OpenAPI transformation, deterministic problem examples, and
  the `MTAPI001` XML-comment projection diagnostic.
- Minimal API and controller error-catalog metadata with immutable owned
  entries, defined-category validation, endpoint-wide duplicate-code rejection,
  and explicit domain error codes/descriptions.
- NativeAOT smoke applications for optional extensions and reflection-free
  OpenAPI package consumption.
- Isolated NativeAOT benchmarks for collections, LINQ, positional patterns,
  ASP.NET Core metadata, and structured-error equality/hashing.

### Changed

- Defined Error equality and hashing by category, numeric category, ordinal
  code/message, disclosure policy, and retained-cause identity instead of
  relying on private record storage.
- Documented the complete public API, pipeline composition, nullable and
  validation boundaries, telemetry/logging ownership, OpenAPI compatibility,
  package selection, and performance tradeoffs.
- Kept Microsoft's complete XML-comment projection as an explicit third-party
  opt-in; the default OpenAPI package remains reflection-free.
- Expanded package verification and package-consumer smoke tests to all ten
  packages, including the default NativeAOT and explicit XML-comment profiles.
- Expanded release-time Windows, Linux, and macOS NativeAOT validation to every
  smoke application and made every gate execute its published binary. The
  OpenAPI smoke additionally starts the trimmed app and generates a catalog
  document.

### Performance

- Retained `0 B` allocation for accepted Result, Option, composition, pattern,
  LINQ, metadata-read, equality, and hashing paths.
- Struct-callable collection traversal measured faster than an
  allocation-equivalent manual loop while owning only the required output
  array.
- Revalidated type-changing `Result.Map` at 2.737 ns and `0 B` against an
  unchanged 2.650 ns same-type control.
- Replaced incremental Error hash construction with fixed-arity scalar mixing,
  reducing the measured NativeAOT path from 58.70 ns to 44.90 ns with `0 B`.

### Tooling And Release Safety

- Enforced strict .NET 10/C# 14 style, analyzers, warnings-as-errors, trimming,
  and NativeAOT compatibility across the solution.
- Removed host-RID contamination from committed package lock files.
- Added a cross-platform C# lock verifier and isolated explicit RID and
  BenchmarkDotNet restores from portable shipping locks without an extra runner.
- Strengthened NativeAOT smoke execution to generate a real OpenAPI document and
  documented the application-owned source-generated JSON metadata requirement.
- Added `AddErrorCatalogOpenApi()` to register package-owned, source-generated
  `ProblemHttpResult` payload metadata without `AddProblemDetails()` services.
- Corrected Git-for-Windows package-smoke HTTPS argument handling.
- Made release tags precede registry publication and allowed safe retries of an
  incomplete release from the same immutable tagged revision.
- Added private-audit ignore rules so application-specific review material
  cannot enter package or repository artifacts accidentally.
- Replaced Bash repository helpers with separate checked-in NativeAOT commands
  for affected-project resolution, lock verification, package creation, and
  package consumption.
- Kept Git paths and lock/Nuspec data in pooled span-based parsers, bounded JSON
  nesting, and removed intermediate path and argument strings.
- Collapsed package creation and dual-profile package consumption into parallel
  single-host MSBuild traversals while requiring a unique package version rather
  than mutating the shared NuGet cache.
- Reduced the measured pack-plus-consume workflow from about 65.26 seconds to
  38.04 seconds for a changed package identity.

[0.2.0-preview.1]: https://github.com/czernous/monadic-types-dotnet/compare/v0.1.0-preview.1...v0.2.0-preview.1
