# Changelog

Notable user-visible changes are recorded here. The project follows Semantic
Versioning as described in [the release policy](docs/releases.md).

## Unreleased

## [0.2.0-preview.2] - 2026-08-30

### Added

- Caller-state `ResultCombination.Map` and `Bind` overloads for two through six
  inputs, avoiding captured projection delegates.
- Caller-state and struct-callable `Option.Match` overloads.
- `ReadOnlySpan<T>.TraverseToArray` delegate, caller-state, and struct-callable
  overloads with the same one-array ownership contract as list traversal.
- Stateful `ValueAction<T,TAction>` wrappers and custom-code/cause overloads for
  `Error.IO` and `Error.System`.
- Test-only compatibility coverage for Vogen-generated value objects.
- A reflection-free documentation tool that derives the complete public API
  inventory from PE metadata and compiler XML, emits a canonical navigable
  reference and search manifest, and synchronizes package README inventories.
- Canonical compiler XML examples for core, optional-package, and
  generated-callable contracts, rendered as highlighted C# blocks in the
  generated API reference.
- Package and release gates that reject stale generated API reference and NuGet
  README output before publication.

### Fixed

- `Option<T>` and `Result<T,E>` equality and hashing now inspect only the active
  case. Empty Options and inactive Result fields no longer invoke generated
  value-object equality or hashing.
- OpenAPI error-catalog collision validation now uses direct packed metadata
  locations instead of repeatedly enumerating preceding entries.
- Larger error catalogs now validate with a bounded stack-backed hash table,
  retaining linear validation for small and collision-heavy catalogs.
- Repeated error categories in endpoint metadata are now emitted once, using a
  bounded bit mask instead of an O(n^2) scan.
- Generated documentation excludes compiler implementation types emitted for
  C# extension blocks, accessor stubs, enum backing fields, and generated record
  machinery, as well as nested types hidden by inaccessible containers, while
  retaining every authored public member.
- Generic XML references, inline-code punctuation, top-level nullable
  annotations, and repository-local Markdown fragments now render and validate
  correctly.
- XML examples use the standard `<example><code>` shape; generated Markdown
  independently emits `csharp` fences while IDE presentation remains controlled
  by each editor.
- Generated references now read and display exact NuGet package IDs, resolve
  assembly/XML pairs exclusively from each project's Release output, use one
  package-anchor policy across Markdown and manifests, and reject empty link
  targets.

### Changed

- `ValueFunction<TIn,TOut,TFunction>` is deeply readonly.
- Diagnostics pass nullable `Error` references by value rather than managed
  by-reference indirection.
- GitHub workflows use checkout v7, upload-artifact v7, and download-artifact v8.

### Performance

- Optimized two-result `Combine` to return the validated operand directly and
  prioritize the dominant success path, reducing the full NativeAOT benchmark
  from 4.0494 ns on the current toolchain to 1.6077 ns with 0 B allocated.

### Not Included

- Matchable case-wrapper hierarchies were rejected because positional
  deconstruction already supports allocation-free pattern matching with less
  API surface, generated code, and NativeAOT size.
- A library-owned XML-comment OpenAPI implementation was rejected. Complete
  projection remains an explicit third-party opt-in because silently adding
  reflection or maintaining a partial compatibility clone would weaken the
  reflection-free default.
- Built-in logging providers, severity mapping, and stack-specific telemetry
  adapters were rejected because disclosure and severity are application
  policies. The retained BCL diagnostics surface and public extension points do
  not require a logging or observability vendor dependency.
- Runtime dependencies on FluentValidation, Vogen, or other domain libraries
  were rejected. Generic conversion APIs and pinned compatibility tests retain
  interoperability without coupling shipping packages to their release cycles.
- Mutable pointer-backed Result state and cached widened errors were rejected
  because mutation, lifetime, retained-exception identity, and concurrency
  semantics conflict with immutable value composition. Widening remains lazy
  and occurs only on the active failure path.
- Eager validation of Result operands after the first failure was rejected
  because it would replace documented fail-fast semantics with a full scan and
  add branches to every successful composition. An uninitialized operand still
  throws when evaluation reaches it.
- Blanket `AggressiveInlining` annotations were rejected because they can
  increase NativeAOT code size or duplicate cold exception paths. Attributes
  are retained or added only where same-layout benchmarks and generated-code
  inspection demonstrate a benefit.
- A general-purpose usage-analyzer package was rejected because policy-driven
  diagnostics add build cost and false-positive risk. Only diagnostics that
  enforce precise package behavior are retained.
- Additional Option convenience operators, composition above arity six, and a
  broad Task/ValueTask side-effect overload matrix were deferred. They add API
  and NativeAOT code-size cost without a recurring measured use case; focused
  caller-state overloads can be added later when profiling proves a gap.
- Accumulating validation and framework-agnostic test helpers were deferred
  until real application use defines stable ownership and assertion semantics.
- Byte-for-byte comparison of rebuilt NativeAOT repository tools was rejected
  because compiler version, absolute paths, and platform linkers do not promise
  reproducible executables. CI instead compiles every changed tool from source
  and executes the relevant behavioral gates.

## [0.2.0-preview.1] - 2026-08-17

### Added

- `MonadicTypes.NET.Collections` with count-known list traversal and span
  sequencing, including delegate, caller-state, and struct-callable selectors.
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

[0.2.0-preview.2]: https://github.com/czernous/monadic-types-dotnet/compare/v0.2.0-preview.1...v0.2.0-preview.2
[0.2.0-preview.1]: https://github.com/czernous/monadic-types-dotnet/compare/v0.1.0-preview.1...v0.2.0-preview.1
