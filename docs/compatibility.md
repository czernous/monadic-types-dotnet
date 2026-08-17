# Compatibility Contract

Runtime projects are reflection-free and are built with the trimming and
NativeAOT analyzers enabled. A composed NativeAOT smoke executable exercises
core results, structured errors, diagnostics, and typed ASP.NET Core results.

## Pinned Compatibility

| Surface | Tested version | Runtime dependency |
| --- | --- | --- |
| .NET and ASP.NET Core | 10.0 | Framework only in `MonadicTypes.AspNetCore` |
| FluentValidation | 12.1.1 | None; compatibility-test only |
| OpenAPI endpoint metadata | ASP.NET Core 10.0 | Framework only in `MonadicTypes.AspNetCore` |
| OpenAPI error-catalog transformation | Microsoft.AspNetCore.OpenApi 10.0.10; Microsoft.OpenApi 2.7.5 | Explicit `MonadicTypes.AspNetCore.OpenApi` package only |

Compatibility tests pin `Microsoft.AspNetCore.TestHost` 10.0.10. The optional
OpenAPI package pins `Microsoft.AspNetCore.OpenApi` 10.0.10 and patched
`Microsoft.OpenApi` 2.7.5; those dependencies do not flow into core or the base
ASP.NET Core package.

The OpenAPI package excludes Microsoft's XML-comment analyzer/build assets in
its dependency metadata and ships a build target that removes the generator if
NuGet resolves it transitively. A direct application reference to
`Microsoft.AspNetCore.OpenApi` is an intentional opt-in; the target detects the
direct reference and preserves Microsoft's generator. The generator's document
transformers use reflection. Our runtime transformer and metadata path do not.

NativeAOT applications must satisfy ASP.NET Core's existing JSON schema
metadata contract by registering every endpoint request, response, and bound
parameter type in an application-owned source-generated `JsonSerializerContext`.
`AddErrorCatalogOpenApi()` supplies source-generated metadata for the
`ProblemHttpResult` payload exposed by this package without registering
problem-details services. The OpenAPI adapter deliberately fails on missing
application metadata instead of silently introducing reflection.

Version changes are deliberate maintenance work. They require compatibility
tests, trim analysis, NativeAOT publication, and benchmark review before the
tested matrix changes.

## Custom Integrations

`Error`, `ValidationErrors`, and `Result<T, E>` are ordinary public values.
Applications may define extension methods that project them directly into a
logging, telemetry, validation, HTTP, or documentation stack. No adapter
interface is required.

`MonadicTypes.Diagnostics` is an optional convenience layer over the BCL
`Activity` and `Meter` APIs. It is not activated automatically and core result
creation or propagation emits no diagnostics. Applications that need different
events or tags should omit that package and implement a local extension.

`MonadicTypes.AspNetCore` provides common RFC problem responses and typed-result
metadata. Generic mapper overloads allow callers to return custom typed results
without converting domain errors or adopting the built-in problem shape.
