# MonadicTypes.NET

High-performance `Result`, `Option`, and production error primitives for .NET
and NativeAOT. Runtime libraries use no reflection, runtime code generation,
service locator, or mandatory dependency-injection integration.

## Projects

- `MonadicTypes`: `Result<T,E>`, `Option<T>`, `Unit`, value functions/actions,
  and source-generator annotations.
- `MonadicTypes.Errors`: structured production errors and validation issues.
- `MonadicTypes.Diagnostics`: optional caller-triggered `Activity` and `Meter`
  projection.
- `MonadicTypes.AspNetCore`: typed HTTP results, RFC problem responses, and
  endpoint response metadata.
- `MonadicTypes.Generators`: compile-time callable wrappers for annotated
  methods; generated wrappers are public and usable outside result pipelines.

## Design

`Result<T,E>` and `Option<T>` are readonly value containers. Their owned state
cannot be mutated, but generic payloads cannot be promised deeply immutable
because C# has no enforceable immutability constraint.

The rich `Error` remains a sealed record reference. This keeps
`Result<T,Error>` compact on the dominant success path. A measured equivalent
32-byte record struct removed the 40 B failure allocation but made successful
mapping 3.93x slower. Allocation-sensitive domains can use compact readonly
struct errors and convert through `IErrorConvertible<Error>` at a transport or
observability boundary.

Delegate overloads provide normal C# ergonomics. Hot paths can use readonly
struct `IValueFunction` implementations or generated wrappers to avoid delegate
dispatch and closure allocation.

## Verification

- NativeAOT and trim analyzers are warnings-as-errors for runtime projects.
- A composed NativeAOT smoke executable exercises core, errors, diagnostics,
  and typed ASP.NET results.
- Compatibility tests pin FluentValidation 12.1.1 without a runtime adapter.
- OpenAPI integration tests pin ASP.NET 10.0.10 and Microsoft.OpenApi 2.7.5.
- Benchmark setup is outside measured operations; accepted composition paths
  allocate 0 B.

See [`docs/compatibility.md`](docs/compatibility.md),
[`docs/benchmarks.md`](docs/benchmarks.md), and
[`docs/extension-roadmap.md`](docs/extension-roadmap.md).

## Status

Experimental and unpublished. The API, implementation, repository structure, and name may change without notice. This repository is maintained primarily for personal experimentation and reuse; it is not currently promoted for broad adoption and carries no support or stability commitment.

## AI-Assisted Development

This project is developed with substantial assistance from AI coding tools. Some code, tests, benchmarks, documentation, and design proposals may be partially or wholly AI-generated. Architecture, acceptance decisions, performance criteria, and published changes remain subject to human direction and review.

## Licensing

No license has been selected. Until a license is added explicitly, publication of this repository does not grant permission to copy, modify, or redistribute its contents.
